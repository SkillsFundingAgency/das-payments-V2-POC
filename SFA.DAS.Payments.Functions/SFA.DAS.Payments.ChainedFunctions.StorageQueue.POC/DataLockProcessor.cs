using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SFA.DAS.Payments.Application.Interfaces;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.ChainedFunctions.StorageQueue.POC
{
    public static class DataLockProcessor
    {
        [FunctionName(nameof(DataLockProcessor))]
        [return:Queue("sequentialvalidation", Connection = "StorageConnectionString")]
        public static EarningValidation Run(
            [QueueTrigger("datalockprocessor", Connection = "StorageConnectionString")]Earning earning,
            TraceWriter log)
        {
            
            // DI
            var commitmentProvider = new CommitmentProvider();
            var accountProvider = new AccountProvider();

            log.Info($"DataLockProcessorStarting.UKPRN={earning.Ukprn},ULN={earning.Uln},LearnerRefNumber={earning.LearnerReferenceNumber}");

            var earnings = new List<Earning>{earning};

            var ukprn = earning.Ukprn;

            var commitments = commitmentProvider.GetCommitments(ukprn, earnings).ToList();
            var accounts = accountProvider.GetAccounts(commitments.Select(x => x.EmployerAccountId).Distinct().ToList());

            log.Info($"DataLockProcessorFinishing.UKPRN={earning.Ukprn},ULN={earning.Uln},LearnerRefNumber={earning.LearnerReferenceNumber}");

            return new EarningValidation(ukprn, commitments, accounts, earning);
        }
    }
}
