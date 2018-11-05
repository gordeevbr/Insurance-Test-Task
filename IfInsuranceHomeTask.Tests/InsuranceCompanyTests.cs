using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using IfInsuranceHomeTask;
using IfInsuranceHomeTask.Exceptions;

namespace IfInsuranceHomeTask.Tests
{
    [TestFixture]
    public class InsuranceCompanyTests
    {
        private IInsuranceCompany InsuranceCompany;

        [SetUp]
        public void SetUP()
        {
            // For the sake of testing, we want to always have the same date used by the insurance company
            // as 'now' in time.
            InsuranceCompany = new InsuranceCompany("Insurance Company", () => new DateTime(2018, 11, 5));
        }

        [Test]
        public void ReturnsPassedRisks()
        {
            var risks = GetMockRisks();

            InsuranceCompany.AvailableRisks = risks;

            Assert.That(InsuranceCompany.AvailableRisks, Is.EqualTo(risks));
        }

        [Test]
        public void DoesNotAllowNullRisks()
        {
            Assert.Throws(typeof(ArgumentNullException), () => InsuranceCompany.AvailableRisks = null);
        }

        [Test]
        public void ReturnsEmptyRisksByDefault()
        {
            Assert.That(InsuranceCompany.AvailableRisks, Is.Empty);
        }

        [Test]
        public void ReturnsAValidCompanyNameByDefault()
        {
            Assert.That(InsuranceCompany.Name, Is.EqualTo("Insurance Company"));
        }

        [Test]
        public void CanSellAValidPolicyOneYear()
        {
            var risks = GetMockRisks();

            InsuranceCompany.AvailableRisks = risks;

            var policy = InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 2, GetMockRisks());

            Assert.That(policy.InsuredRisks, Is.EqualTo(risks));
            Assert.That(policy.NameOfInsuredObject, Is.EqualTo("obj1"));
            Assert.That(policy.Premium, Is.EqualTo(6));
            Assert.That(policy.ValidFrom, Is.EqualTo(new DateTime(2018, 11, 5)));
            Assert.That(policy.ValidTill, Is.EqualTo(new DateTime(2019, 1, 5)));
        }

        [Test]
        public void CanSellAValidPolicyMultipleYears()
        {
            var risks = GetMockRisks();

            InsuranceCompany.AvailableRisks = risks;

            var policy = InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 13, GetMockRisks());

            Assert.That(policy.InsuredRisks, Is.EqualTo(risks));
            Assert.That(policy.NameOfInsuredObject, Is.EqualTo("obj1"));
            Assert.That(policy.Premium, Is.EqualTo(12));
            Assert.That(policy.ValidFrom, Is.EqualTo(new DateTime(2018, 11, 5)));
            Assert.That(policy.ValidTill, Is.EqualTo(new DateTime(2019, 12, 5)));
        }

        [Test]
        public void CanSellAPolicyThatStartsWhenTheOldOneEnds()
        {
            var risks = GetMockRisks();

            InsuranceCompany.AvailableRisks = risks;

            var policy1 = InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 2, risks);

            Assert.That(policy1.InsuredRisks, Is.EqualTo(risks));
            Assert.That(policy1.NameOfInsuredObject, Is.EqualTo("obj1"));
            Assert.That(policy1.Premium, Is.EqualTo(6));
            Assert.That(policy1.ValidFrom, Is.EqualTo(new DateTime(2018, 11, 5)));
            Assert.That(policy1.ValidTill, Is.EqualTo(new DateTime(2019, 1, 5)));

            var policy2 = InsuranceCompany.SellPolicy("obj1", new DateTime(2019, 1, 5), 2, risks);

            Assert.That(policy2.InsuredRisks, Is.EqualTo(risks));
            Assert.That(policy2.NameOfInsuredObject, Is.EqualTo("obj1"));
            Assert.That(policy2.Premium, Is.EqualTo(6));
            Assert.That(policy2.ValidFrom, Is.EqualTo(new DateTime(2019, 1, 5)));
            Assert.That(policy2.ValidTill, Is.EqualTo(new DateTime(2019, 3, 5)));
        }

        [Test]
        public void CanSellTheSamePolicyForADifferentObject()
        {
            var risks = GetMockRisks();

            InsuranceCompany.AvailableRisks = risks;

            var policy1 = InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 2, risks);

            Assert.That(policy1.InsuredRisks, Is.EqualTo(risks));
            Assert.That(policy1.NameOfInsuredObject, Is.EqualTo("obj1"));
            Assert.That(policy1.Premium, Is.EqualTo(6));
            Assert.That(policy1.ValidFrom, Is.EqualTo(new DateTime(2018, 11, 5)));
            Assert.That(policy1.ValidTill, Is.EqualTo(new DateTime(2019, 1, 5)));

            var policy2 = InsuranceCompany.SellPolicy("obj2", new DateTime(2018, 11, 5), 2, risks);

            Assert.That(policy2.InsuredRisks, Is.EqualTo(risks));
            Assert.That(policy2.NameOfInsuredObject, Is.EqualTo("obj2"));
            Assert.That(policy2.Premium, Is.EqualTo(6));
            Assert.That(policy2.ValidFrom, Is.EqualTo(new DateTime(2018, 11, 5)));
            Assert.That(policy2.ValidTill, Is.EqualTo(new DateTime(2019, 1, 5)));
        }

        [Test]
        public void CannotSellAnEmptyPolicy()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            Assert.Throws(typeof(EmptyPolicyException),
                () => InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 1, new List<Risk>()));
        }

        [Test]
        public void CannotSellAPolicyThatStartsInThePast()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            Assert.Throws(typeof(PolicyDateException),
                () => InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 4), 1, GetMockRisks()));
        }

        [Test]
        public void CannotSellAPolicyWithNegativeDuration()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            Assert.Throws(typeof(PolicyDateException),
                () => InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), -1, GetMockRisks()));
        }

        [Test]
        public void CannotSellAPolicyWithoutDuration()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            Assert.Throws(typeof(PolicyDateException),
                () => InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 0, GetMockRisks()));
        }

        [Test]
        public void CannotSellAPolicyWithNullRisks()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            Assert.Throws(typeof(ArgumentNullException),
                () => InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 1, null));
        }

        [Test]
        public void CannotSellADuplicatingPolicy()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 2, GetMockRisks());

            Assert.Throws(typeof(PolicyExistsException),
                () => InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 12, 6), 2, GetMockRisks().Take(2).ToList()));
        }

        [Test]
        public void CanRetreiveASoldPolicy()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            var policy1 = InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 2, GetMockRisks());
            var policy2 = InsuranceCompany.SellPolicy("obj1", new DateTime(2019, 1, 5), 2, GetMockRisks());
            var policy3 = InsuranceCompany.SellPolicy("obj2", new DateTime(2018, 11, 5), 2, GetMockRisks());
            var policy4 = InsuranceCompany.GetPolicy("obj1", new DateTime(2018, 12, 6));

            Assert.AreSame(policy4, policy1);
        }

        [Test]
        public void CannotRetrieveANonExistantPolicyIfThereAreNoPolicies()
        {
            Assert.Null(InsuranceCompany.GetPolicy("obj1", new DateTime(2018, 11, 5)));
        }

        [Test]
        public void CannotRetrieveANonExistantPolicyIfTheObjectIsDifferent()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            var policy = InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 2, GetMockRisks());

            Assert.Null(InsuranceCompany.GetPolicy("obj2", new DateTime(2018, 11, 5)));
        }

        [Test]
        public void CannotRetrieveANonExistantPolicyIfTheDateIsInThePast()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            var policy = InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 2, GetMockRisks());

            Assert.Null(InsuranceCompany.GetPolicy("obj1", new DateTime(2018, 10, 5)));
        }

        [Test]
        public void CannotRetrieveANonExistantPolicyIfTheDateIsInTheFuture()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            var policy = InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 2, GetMockRisks());

            Assert.Null(InsuranceCompany.GetPolicy("obj1", new DateTime(2019, 2, 5)));
        }

        [Test]
        public void CanAddARiskForLessThanOneYear()
        {
            var risks = GetMockRisks();

            InsuranceCompany.AvailableRisks = risks;

            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 13, risks.Take(2).ToList());
            InsuranceCompany.AddRisk("obj1", risks.Skip(2).ToList().First(), new DateTime(2019, 1, 5));

            var policy = InsuranceCompany.GetPolicy("obj1", new DateTime(2018, 11, 5));

            Assert.That(policy.InsuredRisks, Is.EqualTo(risks));
            Assert.That(policy.NameOfInsuredObject, Is.EqualTo("obj1"));
            Assert.That(policy.Premium, Is.EqualTo(9));
            Assert.That(policy.ValidFrom, Is.EqualTo(new DateTime(2018, 11, 5)));
            Assert.That(policy.ValidTill, Is.EqualTo(new DateTime(2019, 12, 5)));
        }

        [Test]
        public void CanAddARiskForMoreThanOneYear()
        {
            var risks = GetMockRisks();

            InsuranceCompany.AvailableRisks = risks;

            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 13, risks.Take(2).ToList());
            InsuranceCompany.AddRisk("obj1", risks.Skip(2).ToList().First(), new DateTime(2018, 12, 4));

            var policy = InsuranceCompany.GetPolicy("obj1", new DateTime(2018, 11, 5));

            Assert.That(policy.InsuredRisks, Is.EqualTo(risks));
            Assert.That(policy.NameOfInsuredObject, Is.EqualTo("obj1"));
            Assert.That(policy.Premium, Is.EqualTo(12));
            Assert.That(policy.ValidFrom, Is.EqualTo(new DateTime(2018, 11, 5)));
            Assert.That(policy.ValidTill, Is.EqualTo(new DateTime(2019, 12, 5)));
        }

        [Test]
        public void CannotAddARiskToANonExistantPolicyIfThereAreNoPolicies()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            Assert.Throws(typeof(PolicyNotFoundException),
                () => InsuranceCompany.AddRisk("obj1", GetMockRisks().First(), new DateTime(2018, 11, 5)));
        }

        [Test]
        public void CannotAddARiskToANonExistantPolicyIfTheObjectNameIsDifferent()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 1, GetMockRisks());

            Assert.Throws(typeof(PolicyNotFoundException),
                () => InsuranceCompany.AddRisk("obj2", GetMockRisks().First(), new DateTime(2018, 11, 5)));
        }

        [Test]
        public void CannotAddARiskToANonExistantPolicyIfTheDateIsInThePast()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 1, GetMockRisks());

            Assert.Throws(typeof(PolicyDateException),
                () => InsuranceCompany.AddRisk("obj1", GetMockRisks().First(), new DateTime(2018, 11, 4)));
        }

        [Test]
        public void CannotAddARiskToANonExistantPolicyIfTheDateIsBeforePolicyStart()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 12, 5), 1, GetMockRisks());

            Assert.Throws(typeof(PolicyDateException),
                () => InsuranceCompany.AddRisk("obj1", GetMockRisks().First(), new DateTime(2018, 11, 5)));
        }

        [Test]
        public void CannotAddARiskToANonExistantPolicyIfTheDateIsAfterPolicyEnd()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 12, 5), 1, GetMockRisks());

            Assert.Throws(typeof(PolicyDateException),
                () => InsuranceCompany.AddRisk("obj1", GetMockRisks().First(), new DateTime(2019, 2, 5)));
        }

        [Test]
        public void CanRemoveARiskForLessThanOneYear()
        {
            var risks = GetMockRisks();

            InsuranceCompany.AvailableRisks = risks;

            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 13, risks.Take(2).ToList());
            InsuranceCompany.RemoveRisk("obj1", risks.Take(1).First(), new DateTime(2019, 2, 2));

            var policy = InsuranceCompany.GetPolicy("obj1", new DateTime(2018, 11, 5));

            Assert.That(policy.InsuredRisks, Is.EqualTo(risks));
            Assert.That(policy.NameOfInsuredObject, Is.EqualTo("obj1"));
            Assert.That(policy.Premium, Is.EqualTo(5));
            Assert.That(policy.ValidFrom, Is.EqualTo(new DateTime(2018, 11, 5)));
            Assert.That(policy.ValidTill, Is.EqualTo(new DateTime(2019, 12, 5)));
        }

        [Test]
        public void CanRemoveARiskForMoreThanOneYear()
        {
            var risks = GetMockRisks();

            InsuranceCompany.AvailableRisks = risks;

            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 13, risks.Take(2).ToList());
            InsuranceCompany.RemoveRisk("obj1", risks.Take(1).First(), new DateTime(2019, 12, 5));

            var policy = InsuranceCompany.GetPolicy("obj1", new DateTime(2018, 11, 5));

            Assert.That(policy.InsuredRisks, Is.EqualTo(risks));
            Assert.That(policy.NameOfInsuredObject, Is.EqualTo("obj1"));
            Assert.That(policy.Premium, Is.EqualTo(4));
            Assert.That(policy.ValidFrom, Is.EqualTo(new DateTime(2018, 11, 5)));
            Assert.That(policy.ValidTill, Is.EqualTo(new DateTime(2019, 12, 5)));
        }


        [Test]
        public void CannotRemoveARiskToANonExistantPolicyIfThereAreNoPolicies()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            Assert.Throws(typeof(PolicyNotFoundException),
                () => InsuranceCompany.RemoveRisk("obj1", GetMockRisks().First(), new DateTime(2018, 11, 5)));
        }

        [Test]
        public void CannotRemoveANonExistantRisk()
        {
            var risks = GetMockRisks();

            InsuranceCompany.AvailableRisks = risks;

            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 13, risks.Take(2).ToList());

            Assert.Throws(typeof(RiskNotFoundException),
                () => InsuranceCompany.RemoveRisk("obj1", GetMockRisks().Skip(2).First(), new DateTime(2018, 12, 5)));
        }

        [Test]
        public void CannotRemoveARiskFromANonExistantPolicyIfTheObjectNameIsDifferent()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 1, GetMockRisks());

            Assert.Throws(typeof(PolicyNotFoundException),
                () => InsuranceCompany.RemoveRisk("obj2", GetMockRisks().First(), new DateTime(2018, 12, 5)));
        }

        [Test]
        public void CannotRemoveARiskFromANonExistantPolicyIfTheDateIsInThePast()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 1, GetMockRisks());

            Assert.Throws(typeof(PolicyDateException),
                () => InsuranceCompany.RemoveRisk("obj1", GetMockRisks().First(), new DateTime(2018, 11, 4)));
        }

        [Test]
        public void CannotRemoveARiskFromANonExistantPolicyIfTheDateIsBeforePolicyStart()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 12, 5), 1, GetMockRisks());

            Assert.Throws(typeof(PolicyDateException),
                () => InsuranceCompany.RemoveRisk("obj1", GetMockRisks().First(), new DateTime(2018, 11, 5)));
        }

        [Test]
        public void CannotRemoveARiskFromANonExistantPolicyIfTheDateIsAfterPolicyEnd()
        {
            InsuranceCompany.AvailableRisks = GetMockRisks();

            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 12, 5), 1, GetMockRisks());

            Assert.Throws(typeof(PolicyDateException),
                () => InsuranceCompany.RemoveRisk("obj1", GetMockRisks().First(), new DateTime(2019, 2, 5)));
        }

        [Test]
        public void CanCreateMultipleRisksOfSameTypeInOnePolicy()
        {
            var risks = GetMockRisks().Take(1).ToList();
            var risk = risks.First();

            InsuranceCompany.AvailableRisks = risks;

            InsuranceCompany.SellPolicy("obj1", new DateTime(2019, 1, 1), 12, risks);
            InsuranceCompany.RemoveRisk("obj1", risk, new DateTime(2019, 2, 1));
            InsuranceCompany.AddRisk("obj1", risk, new DateTime(2019, 2, 1));
            InsuranceCompany.RemoveRisk("obj1", risk, new DateTime(2019, 3, 1));

            var policy = InsuranceCompany.GetPolicy("obj1", new DateTime(2019, 1, 1));

            Assert.That(policy.InsuredRisks, Is.EqualTo(risks));
            Assert.That(policy.NameOfInsuredObject, Is.EqualTo("obj1"));
            Assert.That(policy.Premium, Is.EqualTo(2));
            Assert.That(policy.ValidFrom, Is.EqualTo(new DateTime(2019, 1, 1)));
            Assert.That(policy.ValidTill, Is.EqualTo(new DateTime(2020, 1, 1)));
        }

        [Test]
        public void CannotSellPolicyWithAnUnknownRisk()
        {
            var risks = GetMockRisks();

            InsuranceCompany.AvailableRisks = risks.Take(2).ToList();

            Assert.Throws(typeof(RiskNotFoundException),
                () => InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 1, risks.Skip(2).ToList()));
        }

        [Test]
        public void CannotAddAnUnknownRiskToAPolicy()
        {
            var risks = GetMockRisks();

            InsuranceCompany.AvailableRisks = risks.Take(2).ToList();
            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 1, InsuranceCompany.AvailableRisks);

            Assert.Throws(typeof(RiskNotFoundException),
                () => InsuranceCompany.AddRisk("obj1", risks.Skip(2).ToList().First(), new DateTime(2018, 11, 5)));
        }

        [Test]
        public void CannotRemoveAnUnknownRiskFromAPolicy()
        {
            var risks = GetMockRisks();

            InsuranceCompany.AvailableRisks = risks.Take(2).ToList();
            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 1, InsuranceCompany.AvailableRisks);

            Assert.Throws(typeof(RiskNotFoundException),
                () => InsuranceCompany.RemoveRisk("obj1", risks.Skip(2).ToList().First(), new DateTime(2018, 12, 5)));
        }

        [Test]
        public void CannotRemoveARiskFromTheCompanyWhileItIsBeingUsed()
        {
            var risks = GetMockRisks();

            InsuranceCompany.AvailableRisks = risks;
            InsuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 1, InsuranceCompany.AvailableRisks);

            Assert.Throws(typeof(CannotChangeRisksException),
                () => InsuranceCompany.AvailableRisks = risks.Skip(1).ToList());
        }

        private List<Risk> GetMockRisks()
        {
            var risks = new List<Risk>();

            foreach (int index in Enumerable.Range(1, 3))
            {
                risks.Add(new Risk { Name = $"risk{index}", YearlyPrice = index });
            }

            return risks;
        }
    }
}
