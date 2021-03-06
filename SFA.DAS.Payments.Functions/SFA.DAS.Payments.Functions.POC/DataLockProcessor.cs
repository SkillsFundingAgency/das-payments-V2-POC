using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using SFA.DAS.Payments.Application.Interfaces;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Functions.POC
{
    public static class DataLockProcessor
    {
        [FunctionName(nameof(DataLockProcessor))]
        public static async Task Run(
            [ServiceBusTrigger("datalockprocessor", Connection = "ServiceBusConnection")]
            string myQueueItem,
            [OrchestrationClient] DurableOrchestrationClient client,
            TraceWriter log)
        {
            try
            {
                log.Info("DataLockProcessor Starting");
                // DI
                var commitmentProvider = new CommitmentProvider();
                var accountProvider = new AccountProvider();

                var earnings = JsonConvert.DeserializeObject<List<Earning>>(myQueueItem);

                var ukprn = earnings.Select(x => x.Ukprn).FirstOrDefault();

                var commitments = commitmentProvider.GetCommitments(ukprn, earnings).ToList();
                var accounts = accountProvider.GetAccounts(commitments.Select(x => x.EmployerAccountId).Distinct().ToList());

                var input = new EarningsInput(ukprn, commitments, earnings, accounts);

                await client.StartNewAsync(nameof(EarningsOrchestrator), input);

                log.Info("DataLockProcessor Finishing");
            }
            catch (Exception e)
            {
                log.Error("Error sending learner info", e);
            }
        }
    }
}