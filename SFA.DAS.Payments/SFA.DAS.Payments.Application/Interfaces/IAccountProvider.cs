using System.Collections.Generic;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Application.Interfaces
{
    public interface IAccountProvider
    {
        IReadOnlyList<Account> GetAccounts(long ukprn);

        List<Account> GetAccounts(IReadOnlyList<long> accountIds);
    }
}