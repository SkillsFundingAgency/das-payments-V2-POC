using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock;

namespace SFA.DAS.Payments.Functions.POC
{
    public static class ValidatePathwayCode
    {
        [FunctionName("ValidatePathwayCode")]
        public static Task Run([ActivityTrigger]EarningValidation content)
        {
            var matchResult = content.MatchResult;
            var commitments = matchResult.Commitments;
            var earning = content.Earning;
            if (!earning.StandardCode.HasValue)
            {
                var commitmentsToMatch = commitments == null
                    ? new List<Commitment>()
                    : commitments.Where(c => c.PathwayCode.HasValue &&
                                             earning.PathwayCode.HasValue &&
                                             c.PathwayCode.Equals(earning.PathwayCode.Value)).ToList();

                if (!commitmentsToMatch.Any())
                {
                    matchResult.ErrorCodes.Add(DataLockErrorCodes.MismatchingPathway);
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