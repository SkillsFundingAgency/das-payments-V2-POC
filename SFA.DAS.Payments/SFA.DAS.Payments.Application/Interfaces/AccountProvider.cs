using System.Collections.Generic;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Application.Interfaces
{
    public class AccountProvider : IAccountProvider
    {
        public IReadOnlyList<Account> GetAccounts(long ukprn)
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyList<Account> GetAccounts(IReadOnlyList<long> accountIds)
        {
            throw new System.NotImplementedException();
        }

        public static void SetAccounts(IList<Account> accounts)
        {
        }
    }
}