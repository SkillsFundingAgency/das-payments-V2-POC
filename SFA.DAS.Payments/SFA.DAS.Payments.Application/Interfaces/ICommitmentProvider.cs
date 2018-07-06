using System.Collections.Generic;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Application.Interfaces
{
    public interface ICommitmentProvider
    {
        IReadOnlyList<Commitment> GetCommitments(long ukprn);

        List<Commitment> GetCommitments(long ukprn, IReadOnlyList<string> learnerReferenceNumbers);
    }
}