using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock.Matcher;

namespace SFA.DAS.Payments.Functions.POC
{
    public static class ValidationOrchestrator
    {
        [FunctionName(nameof(ValidationOrchestrator))]
        public static async Task<MatchResult>  Run([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var earning = context.GetInput<EarningValidation>();

            await context.CallActivityAsync(nameof(UlnMatcher), earning)
                .ContinueWith(t => context.CallActivityAsync(nameof(StartDateMatcher), earning))
                .ContinueWith(t => context.CallActivityAsync(nameof(MultipleMatcher), earning))
                .ContinueWith(t => context.CallActivityAsync(nameof(UkprnMatcher), earning));

            await context.CallActivityAsync(nameof(ValidateStandardCode), earning);
            await context.CallActivityAsync(nameof(ValidateFramework), earning);
            await context.CallActivityAsync(nameof(ValidatePathwayCode), earning);
            await context.CallActivityAsync(nameof(ValidateProgrammeType), earning);
            await context.CallActivityAsync(nameof(ValidatePrice), earning);
            await context.CallActivityAsync(nameof(ValidateLevyPayerFlag), earning);

          
            return await Task.FromResult(earning.MatchResult);
        }
    }
}