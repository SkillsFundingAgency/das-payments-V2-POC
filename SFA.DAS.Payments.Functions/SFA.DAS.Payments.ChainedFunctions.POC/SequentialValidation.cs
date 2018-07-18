using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock.Matcher;

namespace SFA.DAS.Payments.ChainedFunctions.POC
{
    public static class SequentialValidation
    {
        [FunctionName(nameof(SequentialValidation))]
        [return: ServiceBus("earningoutput", Connection = "ServiceBusConnection")]
        public static MatchResult Run(
            //[HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, 
            [ServiceBusTrigger("sequentialvalidation", Connection = "ServiceBusConnection")]string request,
            TraceWriter log)
        {
            log.Info("SequentialValidation Starting");

            var factory = MatcherFactory.CreateMatcher();

            //var requestBody = new StreamReader(req.Body).ReadToEnd();

            var input = JsonConvert.DeserializeObject<EarningValidation>(request);

            var result = factory.Match(input.Commitments, input.Earning, input.Accounts);

            log.Info("SequentialValidation Finishing");

            return result;
        }
    }
}