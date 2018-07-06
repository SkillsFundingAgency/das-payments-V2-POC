using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock.Matcher;

namespace SFA.DAS.Payments.Functions.POC
{
    public static class EarningsOrchestrator
    {
        [FunctionName("EarningsOrchestrator")]
        public static async Task<LearnerOutput> Run([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var input = context.GetInput<EarningsInput>();

            var output = new LearnerOutput();

            foreach (var earning in input.Earnings)
            {
                var earningsInput = new EarningValidation(input.Ukprn, input.Commitments, input.Accounts, earning);
                var result = await context.CallSubOrchestratorAsync<MatchResult>("ValidationOrchestrator", earningsInput);

                if (result.Commitments != null && result.Commitments.Any())
                {
                    var success = !result.ErrorCodes.Any();

                    foreach (var commitment in result.Commitments)
                    {
                        if (success)
                        {
                            output.PayableEarnings.Add(new PayableEarning {Commitment = commitment, Earning = earning});
                        }
                        else
                        {
                            output.NonPayableEarnings.Add(new NonPayableEarning {Earning = earning, Errors = result.ErrorCodes});
                        }
                    }
                }
            }

            return output;
        }
    }
}