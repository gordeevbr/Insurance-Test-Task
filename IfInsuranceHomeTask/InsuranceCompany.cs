using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IfInsuranceHomeTask
{
    public class InsuranceCompany : IInsuranceCompany
    {

        private readonly string _Name;
        private readonly Func<DateTime> _DateTimeProvider;

        public InsuranceCompany(string Name = "Insurance Company", Func<DateTime> DateTimeProvider = null)
        {
            this._Name = Name;
            this._DateTimeProvider = DateTimeProvider ?? DefaultDateTimeProvider;
        }

        public IList<Risk> AvailableRisks
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void RemoveRisk(string nameOfInsuredObject, Risk risk, DateTime validTill)
        {
            throw new NotImplementedException();
        }

        public IPolicy SellPolicy(string nameOfInsuredObject, DateTime validFrom, short validMonths, IList<Risk> selectedRisks)
        {
            throw new NotImplementedException();
        }

        private static DateTime DefaultDateTimeProvider()
        {
            return DateTime.Now;
        }
    }
}
