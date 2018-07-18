using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock;

namespace SFA.DAS.Payments.DurableFunctions.POC
{
    public static class StartDateMatcher
    {
        [FunctionName("ValidateStartDate")]
        public static Task Run([ActivityTrigger]EarningValidation content)
        {
            var matchResult = content.MatchResult;
            var commitments = matchResult.Commitments;
            var earning = content.Earning;
            var commitmentsToMatch = commitments == null
                ? new List<Commitment>()
                : commitments.Where(c => earning.StartDate >= c.StartDate
                                         && earning.StartDate < c.EndDate
                                         && earning.StartDate >= c.EffectiveFrom
                                         && (c.EffectiveTo == null || earning.StartDate <= c.EffectiveTo)).ToList();

            if (!commitmentsToMatch.Any())
            {
                matchResult.ErrorCodes.Add(DataLockErrorCodes.EarlierStartDate);
            }
            else
            {
                matchResult.Commitments = commitmentsToMatch.ToArray();
            }

            return Task.FromResult(true);
        }
    }
}