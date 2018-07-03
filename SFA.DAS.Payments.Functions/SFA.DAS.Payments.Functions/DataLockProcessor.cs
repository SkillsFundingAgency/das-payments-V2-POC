using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Functions
{
    public static class DataLockProcessor
    {
        [FunctionName("DataLockProcessor")]
        public static async Task Run(
            [ServiceBusTrigger("datalockprocessor", Connection = "ServiceBusConnection")]
            string myQueueItem,
            [OrchestrationClient] DurableOrchestrationClient client,
            TraceWriter log)
        {
            try
            {

                var earnings = JsonConvert.DeserializeObject<List<Earning>>(myQueueItem);

                await client.StartNewAsync("EarningsOrchestrator", earnings);
            }
            catch (Exception e)
            {
                log.Error("Error sending learner info", e);
            }

        }


    }

    public static class EarningsOrchestrator
    {
      //  [FunctionName("EarningsOrchestrator")]
        public static async Task Run([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var earnings = context.GetInput<List<Earning>>();

            var result = new List<LearnerOutput>();

            var earningsByLearner = earnings.GroupBy(x => x.LearnerReferenceNumber).Select(x => x);

            try
            {
                foreach (var learner in earningsByLearner)
                {
                    result.Add(await context.CallActivityAsync<LearnerOutput>("EarningOrchestrator", learner));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }

    //public static class EarningOrchestrator
    //{
    //    [FunctionName("EarningOrchestrator")]
    //    public static async Task Run([OrchestrationTrigger] DurableOrchestrationContext context)
    //    {
    //        var earnings = context.GetInput<List<Earning>>();

    //        var result = new List<LearnerOutput>();

    //        foreach (var earning in earnings)
    //        {
    //            result.Add(await context.CallActivityAsync<LearnerOutput>("ValidationOrchestrator", earnings));
    //        }
    //    }
    //}

    //public static class ValidationOrchestrator
    //{
    //    [FunctionName("ValidationOrchestrator")]
    //    public static async Task<LearnerOutput> Run([OrchestrationTrigger] DurableOrchestrationContext context)
    //    {
    //        var earning = context.GetInput<Earning>();

    //        var result = new LearnerOutput();

    //        result.PayableEarnings.Add(await context.CallActivityAsync<PayableEarning>("ValidateFramework", result));
    //        result.PayableEarnings.Add(await context.CallActivityAsync<PayableEarning>("ValidateLevyPayerFlag", result));
    //        result.PayableEarnings.Add(await context.CallActivityAsync<PayableEarning>("ValidatePathwayCode", result));
    //        result.PayableEarnings.Add(await context.CallActivityAsync<PayableEarning>("ValidatePrice", result));
    //        result.PayableEarnings.Add(await context.CallActivityAsync<PayableEarning>("ValidateProgrammeType", result));
    //        result.PayableEarnings.Add(await context.CallActivityAsync<PayableEarning>("ValidateStandardCode", result));
    //        result.PayableEarnings.Add(await context.CallActivityAsync<PayableEarning>("ValidateStartDate", result));
    //        result.PayableEarnings.Add(await context.CallActivityAsync<PayableEarning>("ValidateUkprn", result));
    //        result.PayableEarnings.Add(await context.CallActivityAsync<PayableEarning>("ValidateUln", result));

    //        return result;
    //    }
    //}

    public class LearnerOutput
    {
        public LearnerOutput()
        {
            PayableEarnings = new List<PayableEarning>();
            NonPayableEarnings = new List<NonPayableEarning>();
            Exceptions = new List<string>();
        }

        public bool Completed { get; set; } = false;

        public List<PayableEarning> PayableEarnings { get; set; }

        public List<NonPayableEarning> NonPayableEarnings { get; set; }

        public List<string> Exceptions { get; set; }
    }
}

