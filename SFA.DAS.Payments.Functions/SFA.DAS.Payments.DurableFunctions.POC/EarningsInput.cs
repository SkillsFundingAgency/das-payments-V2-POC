using System.Collections.Generic;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Functions.POC
{
    public class EarningsInput
    {
        public EarningsInput(long ukprn, List<Commitment> commitments, List<Earning> earnings, List<Account> accounts)
        {
            Ukprn = ukprn;
            Commitments = commitments;
            Earnings = earnings;
            Accounts = accounts;
        }
        public long Ukprn { get; }

        public List<Commitment> Commitments { get; }

        public List<Earning> Earnings { get; }

        public List<Account> Accounts { get; set; }
    }
}