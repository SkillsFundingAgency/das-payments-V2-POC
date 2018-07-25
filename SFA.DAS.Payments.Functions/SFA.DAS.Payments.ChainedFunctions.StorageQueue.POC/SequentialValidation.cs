using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock.Matcher;

namespace SFA.DAS.Payments.ChainedFunctions.StorageQueue.POC
{
    public static class SequentialValidation
    {
        [FunctionName(nameof(SequentialValidation))]
        [return: Queue("earningoutput", Connection = "StorageConnectionString")]
        public static EarningValidation Run(
            [QueueTrigger("sequentialvalidation", Connection = "StorageConnectionString")]EarningValidation input,
            TraceWriter log)
        {
            var factory = MatcherFactory.CreateMatcher();

            log.Info($"SequentialValidationStarting.UKPRN={input.Earning.Ukprn},ULN={input.Earning.Uln},LearnerRefNumber={input.Earning.LearnerReferenceNumber}");
            var result = factory.Match(input.Commitments, input.Earning, input.Accounts);
        
            input.MatchResult = result;

            log.Info($"SequentialValidationFinishing.UKPRN={input.Earning.Ukprn},ULN={input.Earning.Uln},LearnerRefNumber={input.Earning.LearnerReferenceNumber}");

            return input;
        }
    }
}