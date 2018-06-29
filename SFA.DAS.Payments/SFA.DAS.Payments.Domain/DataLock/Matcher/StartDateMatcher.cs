using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Payments.Domain.DataLock.Matcher
{
    public class StartDateMatcher : MatchHandler
    {
        public StartDateMatcher(MatchHandler nextMatchHandler):
            base(nextMatchHandler)
        {

        }
       
        public override  MatchResult Match(List<Commitment> commitments, Earning priceEpisode, List<Account> dasAccounts, MatchResult matchResult)
        {
            var commitmentsToMatch = commitments.Where(c => priceEpisode.StartDate >= c.StartDate
                                                            && priceEpisode.StartDate < c.EndDate
                                                            && priceEpisode.StartDate >= c.EffectiveFrom 
                                                            && (c.EffectiveTo == null || priceEpisode.StartDate <= c.EffectiveTo)).ToList();

            if (!commitmentsToMatch.Any())
            {
                matchResult.ErrorCodes.Add(DataLockErrorCodes.EarlierStartDate);
                matchResult.Commitments = commitments.ToArray();
            }
            else
            {
                matchResult.Commitments = commitmentsToMatch.ToArray();
            }

            return ExecuteNextHandler(commitmentsToMatch, priceEpisode,dasAccounts,matchResult);
        }
    }
}