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
            var input = context.GetInput<EarningValidation>();

            log.Info($"SequentialValidationStarting.UKPRN={input.Earning.Ukprn},ULN={input.Earning.Uln},LearnerRefNumber={input.Earning.LearnerReferenceNumber}");

            var factory = MatcherFactory.CreateMatcher();

            var result = factory.Match(input.Commitments, input.Earning, input.Accounts);

            log.Info($"SequentialValidationStarting.UKPRN={input.Earning.Ukprn},ULN={input.Earning.Uln},LearnerRefNumber={input.Earning.LearnerReferenceNumber}");
            return Task.FromResult(result);
        }
    }
}