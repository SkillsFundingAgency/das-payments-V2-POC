using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock.Matcher;

namespace SFA.DAS.Payments.DurableFunctions.POC
{
    public static class SequentialValidation
    {
        [FunctionName(nameof(SequentialValidation))]
        public static Task<MatchResult> Run([OrchestrationTrigger] DurableOrchestrationContext context, TraceWriter log)
        {
            log.Info("Validation Starting");
            var input = context.GetInput<EarningValidation>();
            var factory = MatcherFactory.CreateMatcher();

            var result = factory.Match(input.Commitments, input.Earning, input.Accounts);

            log.Info("Validation Finishing");
            return Task.FromResult(result);
        }
    }
}