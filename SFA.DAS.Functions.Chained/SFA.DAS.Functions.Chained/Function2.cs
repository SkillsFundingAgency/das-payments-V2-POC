using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace SFA.DAS.Functions.Chained
{
    public static class Function2
    {
        [FunctionName("Function2")]
        public static void Run([QueueTrigger("function2queue", Connection = "StorageConnectionString")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
