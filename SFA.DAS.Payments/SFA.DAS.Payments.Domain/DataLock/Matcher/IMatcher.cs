using System.Collections.Generic;

namespace SFA.DAS.Payments.Domain.DataLock.Matcher
{
   public interface IMatcher
    {
        MatchResult Match(List<Commitment> commitments, Earning priceEpisode, List<Account> dasAccounts);

    }
}
