using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock;

namespace SFA.DAS.Payments.Functions.POC
{
    public static class UkprnMatcher
    {
        [FunctionName("ValidateUkprn")]
        public static Task Run([ActivityTrigger]EarningValidation content)
        {
            var matchResult = content.MatchResult;
            var commitments = matchResult.Commitments;

            var commitmentsToMatch = commitments == null ? new List<Commitment>() : commitments.Where(c => c.Ukprn.Equals(content.Earning.Ukprn));

            if (!commitmentsToMatch.Any())
            {
                matchResult.ErrorCodes.Add(DataLockErrorCodes.MismatchingUkprn);
            }
            else
            {
                matchResult.Commitments = commitmentsToMatch.ToArray();
            }

            return Task.FromResult(true);
        }
    }
}