using System.Collections.Generic;

namespace SFA.DAS.Payments.Domain
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