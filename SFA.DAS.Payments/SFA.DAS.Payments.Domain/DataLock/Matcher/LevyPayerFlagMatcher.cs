using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Payments.Domain.DataLock.Matcher
{
    public class LevyPayerFlagMatcher : MatchHandler
    {
        public LevyPayerFlagMatcher(MatchHandler nextMatchHandler) :
                base(nextMatchHandler)
        {

        }

        public override MatchResult Match(List<Commitment> commitments, Earning priceEpisode, List<Account> dasAccounts, MatchResult matchResult)
        {
            var commitment = commitments.FirstOrDefault();
            var accountsMatch = dasAccounts.Where(a => commitments.Any(c => c.Id == a.Id && a.IsLevyPayer == true));

            if (!accountsMatch.Any())
            {
                matchResult.ErrorCodes.Add(DataLockErrorCodes.NotLevyPayer);
            }


            return ExecuteNextHandler(commitments, priceEpisode, dasAccounts, matchResult);
        }
    }
}