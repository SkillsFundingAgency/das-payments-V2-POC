using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Payments.Domain.DataLock.Matcher
{
    public class UkprnMatchHandler : MatchHandler

    {
        public UkprnMatchHandler(MatchHandler nextMatchHandler) :
                base(nextMatchHandler)
        {

        }
      
        public override MatchResult Match(List<Commitment> commitments, Earning priceEpisode, List<Account> dasAccounts, MatchResult matchResult)
        {
            var commitmentsToMatch = commitments.Where(c => c.Ukprn == priceEpisode.Ukprn).ToList();

            if (!commitmentsToMatch.Any())
            {
                matchResult.ErrorCodes.Add(DataLockErrorCodes.MismatchingUkprn);
                matchResult.Commitments = commitments.ToArray();
            }
            else
            {
                matchResult.Commitments = commitmentsToMatch.ToArray();
            }

            return ExecuteNextHandler(commitmentsToMatch, priceEpisode,dasAccounts, matchResult);
        }
    }
}