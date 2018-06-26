using System.Collections.Generic;

namespace SFA.DAS.Payments.Domain
{
    public class NonPayableEarning
    {
        public Earning Earning { get; set; }

        public IReadOnlyList<string> Errors { get; set; }
    }
}