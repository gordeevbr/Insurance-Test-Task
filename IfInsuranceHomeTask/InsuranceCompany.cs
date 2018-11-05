using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IfInsuranceHomeTask.Exceptions;

namespace IfInsuranceHomeTask
{
    /// <summary>
    /// A default implementation of the InsuranceCompany.
    /// This implementation is not intended to be thread-safe.
    /// </summary>
    public class InsuranceCompany : IInsuranceCompany
    {

        private readonly string _Name;
        private readonly Func<DateTime> _DateTimeProvider;
        private IList<Risk> _AvailableRisks = new List<Risk>();
        private readonly IList<IPolicy> _Policies = new List<IPolicy>();

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
            throw new NotImplementedException();
        }

        public IPolicy GetPolicy(string nameOfInsuredObject, DateTime effectiveDate)
        {
            return _Policies.FirstOrDefault(it => it.NameOfInsuredObject == nameOfInsuredObject &&
                    InsidePeriod(it.ValidFrom, it.ValidTill, effectiveDate));
        }

        public void RemoveRisk(string nameOfInsuredObject, Risk risk, DateTime validTill)
        {
            throw new NotImplementedException();
        }

        public IPolicy SellPolicy(string nameOfInsuredObject, DateTime validFrom, short validMonths, IList<Risk> selectedRisks)
        {
            if (selectedRisks == null)
            {
                throw new ArgumentNullException("selectedRisks");
            }

            Risk? unavailableRisk = selectedRisks.Cast<Risk?>().FirstOrDefault(it => !_AvailableRisks.Contains(it.Value));
            if (unavailableRisk.HasValue)
            {
                throw new RiskNotFoundException($"The risk {unavailableRisk.Value} is not available");
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

        private static DateTime DefaultDateTimeProvider()
        {
            return DateTime.Now;
        }

        private static bool PeriodsOverlap(DateTime firstStart, DateTime firstEnd, 
            DateTime secondStart, DateTime secondEnd)
        {
            return firstStart < secondEnd && secondStart < firstEnd;
        }

        private static bool InsidePeriod(DateTime start, DateTime end, DateTime point)
        {
            return point >= start && point < end;
        }

        /// <summary>
        /// A policy type used by this implementation of an insurance company.
        /// If I were to implement this interface, I would have negotiated for it to be immutable (without setters).
        /// </summary>
        private class ImmutablePolicy : IPolicy
        {
            private readonly IList<Tuple<DateTime, DateTime?, Risk>> _InsuredRisks;
            private readonly string _NameOfInsuredObject;
            private readonly DateTime _ValidFrom;
            private readonly DateTime _ValidTill;

            public ImmutablePolicy(IList<Tuple<DateTime, DateTime?, Risk>> _InsuredRisks,
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

            private static int YearsDifference(DateTime start, DateTime end)
            {
                return (end.Year - start.Year) +
                    (end.Ticks - new DateTime(end.Year, 1, 1).Ticks >
                    start.Ticks - new DateTime(start.Year, 1, 1).Ticks ? 1 : 0);
            }
        }
    }
}
