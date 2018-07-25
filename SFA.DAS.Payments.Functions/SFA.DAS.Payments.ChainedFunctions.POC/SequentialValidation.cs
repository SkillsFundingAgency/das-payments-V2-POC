using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock.Matcher;

namespace SFA.DAS.Payments.ChainedFunctions.POC
{
    public static class SequentialValidation
    {
        [FunctionName(nameof(SequentialValidation))]
        [return: ServiceBus("earningoutput", Connection = "ServiceBusConnection")]
        public static EarningValidation Run(
            [ServiceBusTrigger("sequentialvalidation", Connection = "ServiceBusConnection")]EarningValidation input,
            TraceWriter log)
        {
            log.Info($"SequentialValidationStarting.UKPRN={input.Earning.Ukprn},ULN={input.Earning.Uln},LearnerRefNumber={input.Earning.LearnerReferenceNumber}");

            var factory = MatcherFactory.CreateMatcher();

            var result = factory.Match(input.Commitments, input.Earning, input.Accounts);

            input.MatchResult = result;

            log.Info($"SequentialValidationFinishing.UKPRN={input.Earning.Ukprn},ULN={input.Earning.Uln},LearnerRefNumber={input.Earning.LearnerReferenceNumber}");

            return input;
        }
    }
}