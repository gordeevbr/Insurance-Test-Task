using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IfInsuranceHomeTask
{
    public class InsuranceCompany : IInsuranceCompany
    {
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
                throw new NotImplementedException();
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
    }
}
