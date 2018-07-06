using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Application.Interfaces
{
    public class AccountProvider : IAccountProvider
    {
        public IReadOnlyList<Account> GetAccounts(long ukprn)
        {
            throw new System.NotImplementedException();
        }

        public List<Account> GetAccounts(IReadOnlyList<long> accountIds)
        {
            return accountIds.Select(accountId => new Account {Id = accountId, IsLevyPayer = true, LevyBalance = 1000000M, TransferBalance = 1000000M}).ToList();
        }

        public static void SetAccounts(IList<Account> accounts)
        {
        }
    }
}