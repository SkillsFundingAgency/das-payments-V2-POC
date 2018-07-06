using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock;

namespace SFA.DAS.Payments.Functions.POC
{
    public static class ValidatePrice
    {
        [FunctionName("ValidatePrice")]
        public static Task Run([ActivityTrigger]EarningValidation content)
        {
            var matchResult = content.MatchResult;
            var earning = content.Earning;
            var commitments = matchResult.Commitments;

            var commitmentsToMatch = commitments == null
                ? new List<Commitment>()
                : commitments.Where(c => earning.NegotiatedPrice.HasValue &&
                                          c.NegotiatedPrice.Equals(earning.NegotiatedPrice.Value)).ToList();

            if (!commitmentsToMatch.Any())
            {
                matchResult.ErrorCodes.Add(DataLockErrorCodes.MismatchingPrice);
            }
            else
            {
                matchResult.Commitments = commitmentsToMatch.ToArray();
            }

            return Task.FromResult(true);
        }
    }
}