using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IfInsuranceHomeTask.Exceptions;

namespace IfInsuranceHomeTask
{
    using RiskTuple = Tuple<DateTime, DateTime?, Risk>;

    /// <summary>
    /// A default implementation of IInsuranceCompany.
    /// This implementation is not intended to be thread-safe.
    /// </summary>
    public class InsuranceCompany : IInsuranceCompany
    {

        private readonly string _Name;
        private readonly Func<DateTime> _DateTimeProvider;
        private IList<Risk> _AvailableRisks = new List<Risk>();
        private readonly IList<ImmutablePolicy> _Policies = new List<ImmutablePolicy>();

        public InsuranceCompany(string Name = "Insurance Company", Func<DateTime> DateTimeProvider = null)
        {
            this._Name = Name;
            this._DateTimeProvider = DateTimeProvider ?? DefaultDateTimeProvider;
        }

        public IList<Risk> AvailableRisks
        {
            get
            {
                return _AvailableRisks;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                var illegallyRemovedRisks = _Policies
                    .SelectMany(it => it.InsuredRisks)
                    .Distinct()
                    .Where(it => !value.Contains(it));
                if (illegallyRemovedRisks.Count() > 0)
                {
                    throw new CannotChangeRisksException();
                }

                _AvailableRisks = value;
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public void AddRisk(string nameOfInsuredObject, Risk risk, DateTime validFrom)
        {
            var policy = CheckIfPolicyExistsFor(nameOfInsuredObject, risk, validFrom, false);

            if (policy.HasRiskLaterThan(risk, validFrom))
            {
                throw new PolicyDateException("Cannot add this risk - policy has another unclosed risk or " +
                    "this risk comes before the latest risk in the policy.");
            }

            var newPolicy = policy.Clone(risks => 
            {
                var newRisks = new List<RiskTuple>(risks);
                newRisks.Add(Tuple.Create(validFrom, new DateTime?(), risk));
                return newRisks;
            });

            _Policies.Remove(policy);
            _Policies.Add(newPolicy);
        }

        public IPolicy GetPolicy(string nameOfInsuredObject, DateTime effectiveDate)
        {
            return GetImmutablePolicy(nameOfInsuredObject, effectiveDate, false);
        }

        public void RemoveRisk(string nameOfInsuredObject, Risk risk, DateTime validTill)
        {
            var policy = CheckIfPolicyExistsFor(nameOfInsuredObject, risk, validTill, true);

            var openRisk = policy.TryFindRemovableRisk(risk, validTill);

            if (openRisk == null)
            {
                throw new RiskNotFoundException($"Can't find a risk that is open and starts before {validTill}");
            }

            var newPolicy = policy.Clone(risks => risks.Select(oldRisk => (oldRisk == openRisk) 
                    ? Tuple.Create(oldRisk.Item1, new DateTime?(validTill), oldRisk.Item3) : oldRisk).ToList());

            _Policies.Remove(policy);
            _Policies.Add(newPolicy);
        }

        public IPolicy SellPolicy(string nameOfInsuredObject, DateTime validFrom, short validMonths, IList<Risk> selectedRisks)
        {
            if (selectedRisks == null)
            {
                throw new ArgumentNullException("selectedRisks");
            }

            var unavailableRisks = selectedRisks.Where(it => !_AvailableRisks.Contains(it));
            if (unavailableRisks.Count() > 0)
            {
                throw new RiskNotFoundException($"Risks [{String.Join(", ", unavailableRisks)}] are not available");
            }

            if (validMonths < 1)
            {
                throw new PolicyDateException($"validMonths must be 1 or more, was {validMonths}");
            }

            if (validFrom < _DateTimeProvider())
            {
                throw new PolicyDateException($"validFrom cannot be in the past, was {validFrom}");
            }

            if (selectedRisks.Count == 0)
            {
                throw new EmptyPolicyException();
            }

            var validTill = validFrom.AddMonths(validMonths);

            if (_Policies.Any(it => it.NameOfInsuredObject == nameOfInsuredObject && 
                    PeriodsOverlap(it.ValidFrom, it.ValidTill, validFrom, validTill)))
            {
                throw new PolicyExistsException();
            }

            var risksWithDurations = selectedRisks.Select(it => Tuple.Create(validFrom, new DateTime?(), it)).ToList();
            var policy = new ImmutablePolicy(risksWithDurations, nameOfInsuredObject, validFrom, validTill);

            _Policies.Add(policy);

            return policy;
        }

        private ImmutablePolicy CheckIfPolicyExistsFor(string nameOfInsuredObject, Risk risk, 
            DateTime dateTime, bool endAllowed)
        {
            if (!_AvailableRisks.Contains(risk))
            {
                throw new RiskNotFoundException($"The risk '{risk.Name}' is not available");
            }

            if (dateTime < _DateTimeProvider())
            {
                throw new PolicyDateException("Cannot add/remove a risk that starts/ends in the past");
            }

            var policy = GetImmutablePolicy(nameOfInsuredObject, dateTime, endAllowed);

            if (policy == null)
            {
                throw new PolicyNotFoundException();
            }

            if (dateTime < policy.ValidFrom
                || ((endAllowed && dateTime > policy.ValidTill) || (!endAllowed && dateTime > policy.ValidTill)))
            {
                throw new PolicyDateException("The provided date does not fall into policies' duration time range.");
            }

            return policy;
        }

        private ImmutablePolicy GetImmutablePolicy(string nameOfInsuredObject, DateTime effectiveDate, bool endAllowed)
        {
            return _Policies
                .Where(it => it.NameOfInsuredObject == nameOfInsuredObject)
                .Where(it => InsidePeriod(it.ValidFrom, it.ValidTill, effectiveDate, endAllowed))
                .FirstOrDefault();
        }

        private static DateTime DefaultDateTimeProvider()
        {
            return DateTime.Now;
        }

        private static bool PeriodsOverlap(DateTime firstStart, DateTime firstEnd, 
            DateTime secondStart, DateTime secondEnd)
        {
            return firstStart < secondEnd && secondStart < firstEnd;
        }

        private static bool InsidePeriod(DateTime start, DateTime end, DateTime point, bool endAllowed)
        {
            return point >= start && ((!endAllowed && point < end) || (endAllowed && point <= end));
        }

        /// <summary>
        /// A policy type used by this implementation of an insurance company.
        /// If I were to implement this interface, I would have negotiated for it to be immutable (without setters).
        /// </summary>
        private class ImmutablePolicy : IPolicy
        {
            private readonly IList<RiskTuple> _InsuredRisks;
            private readonly string _NameOfInsuredObject;
            private readonly DateTime _ValidFrom;
            private readonly DateTime _ValidTill;

            public ImmutablePolicy(IList<RiskTuple> _InsuredRisks,
                string _NameOfInsuredObject,
                DateTime _ValidFrom,
                DateTime _ValidTill)
            {
                this._InsuredRisks = _InsuredRisks;
                this._NameOfInsuredObject = _NameOfInsuredObject;
                this._ValidFrom = _ValidFrom;
                this._ValidTill = _ValidTill;
            }

            public IList<Risk> InsuredRisks
            {
                get
                {
                    return _InsuredRisks.Select(it => it.Item3).Distinct().ToList();
                }

                set
                {
                    throw new InvalidOperationException("This policy is immutable");
                }
            }

            public string NameOfInsuredObject
            {
                get
                {
                    return _NameOfInsuredObject;
                }

                set
                {
                    throw new InvalidOperationException("This policy is immutable");
                }
            }

            public decimal Premium
            {
                get
                {
                    return _InsuredRisks
                        .Select(it => Tuple.Create(it.Item1, it.Item2 ?? ValidTill, it.Item3))
                        .Select(it => Tuple.Create(YearsDifference(it.Item1, it.Item2), it.Item3))
                        .Select(it => it.Item1 * it.Item2.YearlyPrice)
                        .Sum();
                }

                set
                {
                    throw new InvalidOperationException("This policy is immutable");
                }
            }

            public DateTime ValidFrom
            {
                get
                {
                    return _ValidFrom;
                }

                set
                {
                    throw new InvalidOperationException("This policy is immutable");
                }
            }

            public DateTime ValidTill
            {
                get
                {
                    return _ValidTill;
                }

                set
                {
                    throw new InvalidOperationException("This policy is immutable");
                }
            }

            public RiskTuple TryFindRemovableRisk(Risk risk, DateTime validTill)
            {
                return _InsuredRisks
                    .Where(it => !it.Item2.HasValue)
                    .Where(it => it.Item1 <= validTill)
                    .FirstOrDefault();
            }

            public bool HasRiskLaterThan(Risk risk, DateTime validFrom)
            {
                return _InsuredRisks.Any(it => it.Item3.Equals(risk) && 
                    (!it.Item2.HasValue || it.Item1 > validFrom || it.Item2 > validFrom));
            }

            public ImmutablePolicy Clone(Func<IList<RiskTuple>, IList<RiskTuple>> risksMutation)
            {
                return new ImmutablePolicy(risksMutation(_InsuredRisks), _NameOfInsuredObject, _ValidFrom, _ValidTill);
            }

            private static int YearsDifference(DateTime start, DateTime end)
            {
                return (end.Year - start.Year) +
                    (end.Ticks - new DateTime(end.Year, 1, 1).Ticks >
                    start.Ticks - new DateTime(start.Year, 1, 1).Ticks ? 1 : 0);
            }
        }
    }
}
