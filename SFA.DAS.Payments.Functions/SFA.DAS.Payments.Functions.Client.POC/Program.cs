using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Functions.PocClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            const string queueName = "datalockprocessor";
            var connectionString = ConfigurationManager.ConnectionStrings["dataLock"].ConnectionString;

            var client = new QueueClient(connectionString, queueName);


            var exit = false;

            Console.WriteLine("Press Enter to send, Q to quit");

            Console.ReadLine();

            try
            {
                while (!exit)
                {
                    var learners = TestDataGenerator.TestDataGenerator.CreateEarningsFromLearners();

                    const int batch = 100;
                    var batchSize = 0;
                    var current = learners.Skip(batchSize).Take(batch).ToList();

                    while (current.Any())
                    {
                        var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(current)));

                        client.SendAsync(message);

                        batchSize += batch;

                        current = learners.Skip(batchSize).Take(batch).ToList();

                        current = new List<Earning>();
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
