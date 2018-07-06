using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace SFA.DAS.Payments.Functions.POC
{
    public static class LearnerOrchestrator
    {
        [FunctionName("LearnerOrchestrator")]
        public static async Task Run([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var input = context.GetInput<EarningsInput>();

            var result = new List<LearnerOutput>();

            var earningsByLearner = input.Earnings.GroupBy(x => x.LearnerReferenceNumber).Select(x => x);

            try
            {
                foreach (var learner in earningsByLearner)
                {
                    var learnerCommitments = input.Commitments.Where(x => x.LearnerReferenceNumber == learner.Key).ToList();
                    var learnerAccounts = input.Accounts.Where(l => learnerCommitments.Select(x => x.Id).Contains(l.Id)).ToList();
                    var earningInput = new EarningsInput(input.Ukprn,learnerCommitments , learner.ToList(), learnerAccounts);
                    result.Add(await context.CallSubOrchestratorAsync<LearnerOutput>("EarningsOrchestrator", earningInput));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine(JsonConvert.SerializeObject(result));
        }
    }
}