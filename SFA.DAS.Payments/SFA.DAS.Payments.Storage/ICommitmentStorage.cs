using System.Collections.Generic;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Storage
{
    public interface ICommitmentStorage
    {
        void Store(IList<Commitment> commitments);
        Commitment Get(string key);
    }
}
