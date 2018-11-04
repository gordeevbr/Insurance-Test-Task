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
        private IInsuranceCompany insuranceCompany;

        [SetUp]
        public void SetUP()
        {
            // For the sake of testing, we want to always have the same date used by the insurance company
            // as 'now' in time.
            insuranceCompany = new InsuranceCompany("Insurance Company", () => new DateTime(2018, 11, 5));
        }

        [Test]
        public void ReturnsPassedRisks()
        {
            var risks = new List<Risk>();

            foreach (int index in Enumerable.Range(1, 3))
            {
                risks.Add(new Risk { Name = $"risk{index}", YearlyPrice = index });
            }

            insuranceCompany.AvailableRisks = risks;

            Assert.That(insuranceCompany.AvailableRisks, Is.EqualTo(risks));
        }

        [Test]
        public void ReturnsEmptyRisksByDefault()
        {
            Assert.That(insuranceCompany.AvailableRisks, Is.Empty);
        }

        [Test]
        public void ReturnsAValidCompanyNameByDefault()
        {
            Assert.That(insuranceCompany.Name, Is.EqualTo("Insurance Company"));
        }

        [Test]
        public void CanSellAValidPolicy()
        {
            var risks = new List<Risk>();

            foreach (int index in Enumerable.Range(1, 3))
            {
                risks.Add(new Risk { Name = $"risk{index}", YearlyPrice = index });
            }

            insuranceCompany.AvailableRisks = risks;

            var policy = insuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 1, risks);

            Assert.That(policy.InsuredRisks, Is.EqualTo(risks));
            Assert.That(policy.NameOfInsuredObject, Is.EqualTo("obj1"));
            Assert.That(policy.Premium, Is.EqualTo(6));
            Assert.That(policy.ValidFrom, Is.EqualTo(new DateTime(2018, 11, 5)));
            Assert.That(policy.ValidTill, Is.EqualTo(new DateTime(2019, 12, 5)));
        }

        public void CannotSellAnEmptyPolicy()
        {
            Assert.Throws(typeof(EmptyPolicyException),
                () => insuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 5), 1, new List<Risk>()));
        }

        public void CannotSellAPolicyThatStartsInThePast()
        {
            var risks = new List<Risk>();

            foreach (int index in Enumerable.Range(1, 3))
            {
                risks.Add(new Risk { Name = $"risk{index}", YearlyPrice = index });
            }

            insuranceCompany.AvailableRisks = risks;

            Assert.Throws(typeof(PolicyDateException),
                () => insuranceCompany.SellPolicy("obj1", new DateTime(2018, 11, 4), 1, risks));
        }
    }
}
