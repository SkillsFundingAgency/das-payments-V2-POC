using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.ChainedFunctions.StorageQueue.POC
{
    public static class EarningOutput
    {
        [FunctionName(nameof(EarningOutput))]
        [return: Queue("learneroutput", Connection = "StorageConnectionString")]
        public static LearnerOutput Run(
            [QueueTrigger("earningoutput", Connection = "StorageConnectionString")]string request,
            TraceWriter log)
        {
            log.Info("EarningOutput Starting");

            var result = JsonConvert.DeserializeObject<EarningValidation>(request);

            var output = new LearnerOutput();

            var matchResult = result.MatchResult;
            var earning = result.Earning;

            if (matchResult.Commitments != null && matchResult.Commitments.Any())
            {
                log.Info("Validation result retrieved");
                var success = !matchResult.ErrorCodes.Any();

                foreach (var commitment in matchResult.Commitments)
                {
                    if (success)
                    {
                        log.Info("Payable earning added");
                        output.PayableEarnings.Add(new PayableEarning { Commitment = commitment, Earning = earning });
                    }
                    else
                    {
                        log.Info("Non-Payable earning added");
                        output.NonPayableEarnings.Add(new NonPayableEarning { Earning = earning, Errors = matchResult.ErrorCodes });
                    }
                }
            }

            return output;
        }
    }
}
