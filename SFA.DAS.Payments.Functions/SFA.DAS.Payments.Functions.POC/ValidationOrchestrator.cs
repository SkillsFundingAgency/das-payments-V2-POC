using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock.Matcher;

namespace SFA.DAS.Payments.Functions.POC
{
    public static class ValidationOrchestrator
    {
        [FunctionName("ValidationOrchestrator")]
        public static async Task<MatchResult>  Run([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var earning = context.GetInput<EarningValidation>();

            await context.CallActivityAsync("ValidateUln", earning)
                .ContinueWith(t => context.CallActivityAsync("ValidateStartDate", earning))
                .ContinueWith(t => context.CallActivityAsync("ValidateMultipleMatch", earning))
                .ContinueWith(t => context.CallActivityAsync("ValidateUkprn", earning))
                .ContinueWith(t => context.CallActivityAsync("ValidateStandardCode", earning))
                .ContinueWith(t => context.CallActivityAsync("ValidateFramework", earning))
                .ContinueWith(t => context.CallActivityAsync("ValidatePathwayCode", earning))
                .ContinueWith(t => context.CallActivityAsync("ValidateProgrammeType", earning))
                .ContinueWith(t => context.CallActivityAsync("ValidatePrice", earning))
                .ContinueWith(t => context.CallActivityAsync("ValidateLevyPayerFlag", earning));

            /*
            await context.CallActivityAsync("ValidateUln", earning);
            await context.CallActivityAsync("ValidateStartDate", earning);
            await context.CallActivityAsync("ValidateMultipleMatch", earning);
            await context.CallActivityAsync("ValidateUkprn", earning);

            await context.CallActivityAsync("ValidateStandardCode", earning);
            await context.CallActivityAsync("ValidateFramework", earning);
            await context.CallActivityAsync("ValidatePathwayCode", earning);
            await context.CallActivityAsync("ValidateProgrammeType", earning);
            await context.CallActivityAsync("ValidatePrice", earning);
            await context.CallActivityAsync("ValidateLevyPayerFlag", earning);
            */

            return await Task.FromResult(earning.MatchResult);
        }
    }
}