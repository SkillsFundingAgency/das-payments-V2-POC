using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock;

namespace SFA.DAS.Payments.Functions.POC
{
    public static class ValidateMultipleMatch
    {
        [FunctionName("ValidateMultipleMatch")]
        public static Task Run([ActivityTrigger]EarningValidation content)
        {
            var matchResult = content.MatchResult;
            var commitments = matchResult.Commitments;
            var distinctCommitmentIds = commitments == null
                ? new List<Commitment>()
                : commitments.Where(x => x.PaymentStatus != (int) PaymentStatus.Cancelled
                                          && x.PaymentStatus != (int) PaymentStatus.Deleted
                                          && x.PaymentStatus != (int) PaymentStatus.Completed)
                    .Distinct();

            if (distinctCommitmentIds.Count() > 1)
            {
                matchResult.ErrorCodes.Add(DataLockErrorCodes.MultipleMatches);
            }

            return Task.FromResult(true);
        }
    }
}