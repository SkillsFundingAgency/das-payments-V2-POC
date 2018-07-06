using System.Collections.Generic;
using SFA.DAS.Payments.Domain.DataLock.Matcher;

namespace SFA.DAS.Payments.Domain
{
    public class EarningValidation
    {
        public EarningValidation(long ukprn, List<Commitment> commitments, List<Account> accounts, Earning earning)
        {
            Ukprn = ukprn;
            Commitments = commitments;
            Accounts = accounts;
            Earning = earning;
            MatchResult = new MatchResult
            {
                Commitments = commitments.ToArray()
            };
        }

        public long Ukprn { get;  }

        public Earning Earning { get; set; }

        public List<Commitment> Commitments { get; set; }

        public List<Account> Accounts { get; set; }

        public MatchResult MatchResult { get; set; } 
    }
}