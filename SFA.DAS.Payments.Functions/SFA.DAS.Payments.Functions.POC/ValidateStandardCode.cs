using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock;

namespace SFA.DAS.Payments.Functions.POC
{
    public static class ValidateStandardCode
    {
        [FunctionName("ValidateStandardCode")]
        public static Task Run([ActivityTrigger]EarningValidation content)
        {
            var matchResult = content.MatchResult;
            var earning = content.Earning;
            var commitments = matchResult.Commitments;

            if (earning.StandardCode.HasValue)
            {
                var commitmentsToMatch = commitments?.Where(c => c.StandardCode.HasValue &&
                                                                c.StandardCode.Equals(earning.StandardCode)).ToList();

                if (!commitmentsToMatch.Any())
                {
                    matchResult.ErrorCodes.Add(DataLockErrorCodes.MismatchingStandard);
                }
                else
                {
                    matchResult.Commitments = commitmentsToMatch.ToArray();
                }
            }

            return Task.FromResult(true);
        }
    }
}