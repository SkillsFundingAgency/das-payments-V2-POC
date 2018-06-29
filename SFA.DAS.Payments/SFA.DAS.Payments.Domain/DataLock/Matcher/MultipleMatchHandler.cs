using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Payments.Domain.DataLock.Matcher
{
    public class MultipleMatchHandler : MatchHandler
    {
    
        public MultipleMatchHandler(MatchHandler nextMatchHandler):
            base(nextMatchHandler)
        {

        }
        public override MatchResult Match(List<Commitment> commitments, Earning priceEpisode, List<Account> dasAccounts, MatchResult matchResult)
        {
            var distinctCommitmentIds = commitments
                .Where(x=> x.PaymentStatus != (int)PaymentStatus.Cancelled 
                        && x.PaymentStatus != (int)PaymentStatus.Deleted
                        && x.PaymentStatus != (int)PaymentStatus.Completed)
                .Select(c => new {Id = c.Id})
                .Distinct()
                .ToArray();

            if (distinctCommitmentIds.Length > 1)
            {
                matchResult.ErrorCodes.Add(DataLockErrorCodes.MultipleMatches);
            }
           
            matchResult.Commitments = commitments.ToArray();

            return ExecuteNextHandler(commitments, priceEpisode,dasAccounts, matchResult);
        }
    }
}