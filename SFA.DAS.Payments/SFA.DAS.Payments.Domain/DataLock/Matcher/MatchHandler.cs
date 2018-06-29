using System.Collections.Generic;

namespace SFA.DAS.Payments.Domain.DataLock.Matcher
{
    public abstract class MatchHandler : IMatcher
    {
        public virtual bool StopOnError
        {
            get
            {
                return true;
            }
        }

        protected MatchHandler NextMatchHandler;

        protected MatchHandler(MatchHandler nextMatchHandler)
        {
            NextMatchHandler = nextMatchHandler;
        }

        public MatchResult Match(List<Commitment> commitments, Earning priceEpisode, List<Account> dasAccounts)
        {
            return Match(commitments, priceEpisode,dasAccounts, new MatchResult() );
        }

        public abstract MatchResult Match(List<Commitment> commitments, Earning priceEpisode, List<Account> dasAccounts, MatchResult matchResult);

        protected MatchResult ExecuteNextHandler(List<Commitment> commitments, Earning priceEpisode, List<Account> dasAccounts, MatchResult matchResult)
        {
           
            return NextMatchHandler == null || (StopOnError && matchResult.ErrorCodes.Count > 0)
                ? matchResult
                : NextMatchHandler.Match(commitments, priceEpisode,dasAccounts, matchResult);
        }

    }
}