using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IfInsuranceHomeTask.Exceptions
{
    public class PolicyDateException: ArgumentException
    {
        public PolicyDateException(string message): base(message)
        {
        }
    }
}
