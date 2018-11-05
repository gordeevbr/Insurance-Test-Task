﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IfInsuranceHomeTask.Exceptions
{
    public class RiskNotFoundException: ArgumentException
    {
        public RiskNotFoundException(string message) : base(message)
        {
        }
    }
}
