using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Payments.Domain
{
    public class Metric
    {
        public int Number { get; set; }
        public long InsideRead { get; set; }
        public long InsideCalc { get; set; }
        public long InsideWrite { get; set; }
        public long OutsideProxy { get; set; }
        public long OutsideCall { get; set; }
        public string Actor { get; set; }
    }
}
