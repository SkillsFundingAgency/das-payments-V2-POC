using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using SFA.DAS.Payments.Application.Interfaces;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock.Matcher;

namespace SFA.DAS.Payments.ChainedFunctions.POC
{
    public static class SingularDataLockProcessor
    {
        [FunctionName(nameof(SingularDataLockProcessor))]
        [return: ServiceBus("learneroutput", Connection = "ServiceBusConnection")]
        public static LearnerOutput Run(
            [ServiceBusTrigger("singulardatalockprocessor", Connection = "ServiceBusConnection")]string request,
            TraceWriter log)
        {

            // DI
            var commitmentProvider = new CommitmentProvider();
            var accountProvider = new AccountProvider();

            var earning = JsonConvert.DeserializeObject<Earning>(request);

            var enqueueTime = earning.EnqueueTime;
            var startTime = DateTime.Now;

            var earnings = new List<Earning> { earning };

            var ukprn = earning.Ukprn;

            var commitments = commitmentProvider.GetCommitments(ukprn, earnings).ToList();
            var accounts = accountProvider.GetAccounts(commitments.Select(x => x.EmployerAccountId).Distinct().ToList());

            var factory = MatcherFactory.CreateMatcher();

            var validation = new EarningValidation(ukprn, commitments, accounts, earning)
            {
                MatchResult = factory.Match(commitments, earning, accounts)
            };

            var output = new LearnerOutput();

            var matchResult = validation.MatchResult;

            if (matchResult.Commitments != null && matchResult.Commitments.Any())
            {
                var success = !matchResult.ErrorCodes.Any();

                foreach (var commitment in matchResult.Commitments)
                {
                    if (success)
                    {
                        output.PayableEarnings.Add(new PayableEarning { Commitment = commitment, Earning = earning });
                    }
                    else
                    {
                        output.NonPayableEarnings.Add(new NonPayableEarning { Earning = earning, Errors = matchResult.ErrorCodes });
                    }
                }
            }

            var endTime = DateTime.Now;

            log.Info($"SequentialValidationFinishing.UKPRN={earning.Ukprn},ULN={earning.Uln},LearnerRefNumber={earning.LearnerReferenceNumber},EnqueueTime={enqueueTime:HH:mm:ss.fff},StartTime={startTime:HH:mm:ss.fff},EndTime={endTime:HH:mm:ss.fff}");
            return output;
        }
    }
}