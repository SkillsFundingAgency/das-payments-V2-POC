using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Application.Interfaces
{
    public class EarningProvider : IEarningProvider
    {
        private static IList<Earning> _earnings;

        public IReadOnlyList<Earning> GetEarnings()
        {
            return _earnings.ToArray();
        }

        public static void SetEarnings(IList<Earning> earnings)
        {
            _earnings = earnings;
        }
    }
}