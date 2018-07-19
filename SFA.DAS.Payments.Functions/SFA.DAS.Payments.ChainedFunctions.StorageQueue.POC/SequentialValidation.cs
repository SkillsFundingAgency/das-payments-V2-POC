using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock.Matcher;

namespace SFA.DAS.Payments.ChainedFunctions.StorageQueue.POC
{
    public static class SequentialValidation
    {
        [FunctionName(nameof(SequentialValidation))]
        [return: Queue("earningoutput", Connection = "StorageConnectionString")]
        public static MatchResult Run(
            [QueueTrigger("sequentialvalidation", Connection = "StorageConnectionString")]string request,
            TraceWriter log)
        {
            log.Info("SequentialValidation Starting");

            var factory = MatcherFactory.CreateMatcher();

            var input = JsonConvert.DeserializeObject<EarningValidation>(request);

            var result = factory.Match(input.Commitments, input.Earning, input.Accounts);

            log.Info("SequentialValidation Finishing");

            return result;
        }
    }
}