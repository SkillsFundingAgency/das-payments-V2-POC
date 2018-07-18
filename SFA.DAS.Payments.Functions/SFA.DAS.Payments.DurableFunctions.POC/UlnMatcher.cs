using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock;

namespace SFA.DAS.Payments.DurableFunctions.POC
{
    public static class UlnMatcher
    {
        [FunctionName(nameof(UlnMatcher))]
        public static Task Run([ActivityTrigger]EarningValidation content)
        {
            var match = content.MatchResult;
            var commitmentsToMatch = content.Commitments?.Where(c => content.Earning.Uln.HasValue && c.Uln.Equals(content.Earning.Uln.Value)).ToList();

            if (!commitmentsToMatch.Any())
            {
                match.ErrorCodes.Add(DataLockErrorCodes.MismatchingUln);
            }
            else
            {
                match.Commitments = commitmentsToMatch.ToArray();
            }

            return Task.FromResult(true);
        }
    }
}