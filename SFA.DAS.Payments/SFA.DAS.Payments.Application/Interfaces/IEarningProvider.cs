using System.Collections.Generic;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Application.Interfaces
{
    public interface IEarningProvider
    {
        IReadOnlyList<Earning> GetEarnings();
    }
}