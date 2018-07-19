﻿using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace SFA.DAS.Payments.ChainedFunctions.StorageQueue.Client.POC
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            const string queueName = "datalockprocessor";
            var connectionString = ConfigurationManager.ConnectionStrings["dataLock"].ConnectionString;

            var storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the queue client.
            var queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a container.
            var queue = queueClient.GetQueueReference(queueName);

            // Create the queue if it doesn't already exist
            queue.CreateIfNotExistsAsync();

            var exit = false;

            Console.WriteLine("Press Enter to send, Q to quit");

            Console.ReadLine();

            try
            {
                while (!exit)
                {
                    var earnings = TestDataGenerator.TestDataGenerator.CreateEarningsFromLearners();

                    foreach (var earning in earnings.Take(100))
                    {
                        var message = new CloudQueueMessage(JsonConvert.SerializeObject(earning));

                        queue.AddMessageAsync(message);
                    }

                    var res = Console.ReadLine();
                    if (res == "Q")
                    {
                        exit = true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}