using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.ChainedFunctions.POC
{
    public static class EarningOutput
    {
        [FunctionName("earningoutput")]
        [return: ServiceBus("learneroutput", Connection = "ServiceBusConnection")]
        public static LearnerOutput Run(
            [ServiceBusTrigger("earningoutput", Connection = "ServiceBusConnection")]EarningValidation result,
            TraceWriter log)
        {
            log.Info($"EarningOutputStarting.UKPRN={result.Earning.Ukprn},ULN={result.Earning.Uln},LearnerRefNumber={result.Earning.LearnerReferenceNumber}");

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

            log.Info($"EarningOutputStarting.UKPRN={result.Earning.Ukprn},ULN={result.Earning.Uln},LearnerRefNumber={result.Earning.LearnerReferenceNumber}");

            return output;
        }
    }
}
