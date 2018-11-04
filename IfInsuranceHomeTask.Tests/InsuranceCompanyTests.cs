using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using IfInsuranceHomeTask;

namespace IfInsuranceHomeTask.Tests
{
    [TestFixture]
    public class InsuranceCompanyTests
    {
        private IInsuranceCompany insuranceCompany;

        [SetUp]
        public void SetUP()
        {
            insuranceCompany = new InsuranceCompany();
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
        public void ReturnsAValidCompanyNameByDefault()
        {
            Assert.That(insuranceCompany.Name, Is.EqualTo("Insurance Company"));
        }
    }
}
