using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock;

namespace SFA.DAS.Payments.DurableFunctions.POC
{
    public static class ValidateProgrammeType
    {
        [FunctionName("ValidateProgrammeType")]
        public static Task Run([ActivityTrigger]EarningValidation content)
        {
            var matchResult = content.MatchResult;
            var earning = content.Earning;
            var commitments = matchResult.Commitments;

            if (!earning.StandardCode.HasValue)
            {
                var commitmentsToMatch = commitments == null? new List<Commitment>() : commitments.Where(c => c.ProgrammeType.HasValue &&
                                                                earning.ProgrammeType.HasValue &&
                                                                c.ProgrammeType.Equals(earning.ProgrammeType)).ToList();

                if (!commitmentsToMatch.Any())
                {
                    matchResult.ErrorCodes.Add(DataLockErrorCodes.MismatchingProgramme);
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