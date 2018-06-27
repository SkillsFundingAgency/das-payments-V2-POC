using System.Collections.Generic;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Storage
{
    public interface IEarningStorage
    {
        void Store(IList<Earning> earnings);
        Earning Get(string key);
    }
}