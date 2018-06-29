using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Payments.Domain.DataLock.Matcher
{
    public class FrameworkMatchHandler : MatchHandler
    {
        public override bool StopOnError
        {
            get
            {
                return false;
            }
        }
        public FrameworkMatchHandler(MatchHandler nextMatchHandler):
            base(nextMatchHandler)
        {
           
        }

        public override MatchResult Match(List<Commitment> commitments, Earning priceEpisode, List<Account> dasAccounts, MatchResult matchResult)
        {
            matchResult.Commitments = commitments.ToArray();

            if (!priceEpisode.StandardCode.HasValue)
            {
                var commitmentsToMatch = commitments.Where(c => c.FrameworkCode.HasValue &&
                                                                priceEpisode.FrameworkCode.HasValue &&
                                                                c.FrameworkCode.Value == priceEpisode.FrameworkCode.Value)
                    .ToList();

                if (!commitmentsToMatch.Any())
                {
                    matchResult.ErrorCodes.Add(DataLockErrorCodes.MismatchingFramework);
                }
                else
                {
                    matchResult.Commitments = commitmentsToMatch.ToArray();
                }

            }

            return ExecuteNextHandler(commitments, priceEpisode,dasAccounts, matchResult);
        }
    }
}