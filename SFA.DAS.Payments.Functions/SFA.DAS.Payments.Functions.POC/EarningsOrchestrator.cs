using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock.Matcher;

namespace SFA.DAS.Payments.Functions.POC
{
    public static class EarningsOrchestrator
    {
        [FunctionName(nameof(EarningsOrchestrator))]
        public static async Task<LearnerOutput> Run([OrchestrationTrigger] DurableOrchestrationContext context, TraceWriter log)
        {
            try
            {
                log.Info("Earnings Orchestrator Starting");
                var input = context.GetInput<EarningsInput>();

                var output = new LearnerOutput();

                var inputEarnings = new List<Earning> {input.Earnings.First()};

                foreach (var earning in inputEarnings)
                {

                    var learnerAccounts = input.Accounts.Where(l => input.Commitments.Select(x => x.EmployerAccountId).Contains(l.Id)).ToList();

                    var earningsInput = new EarningValidation(input.Ukprn, input.Commitments, learnerAccounts, earning);
                    var result = await context.CallSubOrchestratorAsync<MatchResult>(nameof(SequentialValidation), earningsInput);

                    if (result.Commitments != null && result.Commitments.Any())
                    {
                        var success = !result.ErrorCodes.Any();

                        foreach (var commitment in result.Commitments)
                        {
                            if (success)
                            {
                                log.Info("Payable earning added");
                                output.PayableEarnings.Add(new PayableEarning {Commitment = commitment, Earning = earning});
                            }
                            else
                            {
                                log.Info("Non-Payable earning added");
                                output.NonPayableEarnings.Add(new NonPayableEarning {Earning = earning, Errors = result.ErrorCodes});
                            }
                        }
                    }
                }

                log.Info("Earnings Orchestrator Finishing");
                return output;
            }
            catch (Exception ex)
            {
                log.Info(ex.Message);

                return null;
            }
        }
    }
}