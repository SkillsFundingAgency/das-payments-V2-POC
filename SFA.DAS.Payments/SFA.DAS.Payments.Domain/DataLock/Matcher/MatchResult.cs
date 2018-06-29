using System.Collections.Generic;

namespace SFA.DAS.Payments.Domain.DataLock.Matcher
{
    public class MatchResult
    {
   
        public List<string> ErrorCodes { get; set; } = new List<string>();
        public Commitment[] Commitments { get; set; } 
    }
}