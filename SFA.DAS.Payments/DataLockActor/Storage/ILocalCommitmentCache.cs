using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Payments.Domain;

namespace DataLockActor
{
    public interface ILocalCommitmentCache
    {
        Task Reset();
        Task Add(string key, IList<Commitment> commitments);
        Task<IList<Commitment>> Get(string key);
        Task Update(string key, List<Commitment> commitments);
    }
}
