using System.Collections.Generic;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Functions.POC
{
    public class LearnerOutput
    {
        public LearnerOutput()
        {
            PayableEarnings = new List<PayableEarning>();
            NonPayableEarnings = new List<NonPayableEarning>();
            Exceptions = new List<string>();
        }

        public bool Completed { get; set; } = false;

        public List<PayableEarning> PayableEarnings { get; set; }

        public List<NonPayableEarning> NonPayableEarnings { get; set; }

        public List<string> Exceptions { get; set; }
    }
}