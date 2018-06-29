using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Payments.Domain.DataLock.Matcher
{
    public class PriceMatchHandler : MatchHandler
    {
        public PriceMatchHandler(MatchHandler nextMatchHandler):
            base(nextMatchHandler)
        {

        }
        public override bool StopOnError
        {
            get
            {
                return false;
            }
        }
        public override MatchResult Match(List<Commitment> commitments, Earning priceEpisode, List<Account> dasAccounts, MatchResult matchResult)
        {
            matchResult.Commitments = commitments.ToArray();

            var commitmentsToMatch = commitments.Where(c => priceEpisode.NegotiatedPrice.HasValue &&
                                                            (long) c.NegotiatedPrice == priceEpisode.NegotiatedPrice.Value).ToList();

            if (!commitmentsToMatch.Any())
            {
               matchResult.ErrorCodes.Add(DataLockErrorCodes.MismatchingPrice);
            }
        
            else
            {
                matchResult.Commitments = commitmentsToMatch.ToArray();
            }

            return ExecuteNextHandler(commitments, priceEpisode,dasAccounts,matchResult);
        }
    }
}