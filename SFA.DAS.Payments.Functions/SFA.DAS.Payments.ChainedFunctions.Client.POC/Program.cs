using System;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace SFA.DAS.Payments.ChainedFunctions.Client.POC
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Press D to send all messages to multi-function method and S for single function");

            var method = Console.ReadLine();

            var queueName = method == "D" ? "DataLockProcessor" : "singulardatalockprocessor";

            var connectionString = ConfigurationManager.ConnectionStrings["dataLock"].ConnectionString;

            var client = new QueueClient(connectionString, queueName);

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
                        earning.EnqueueTime = DateTime.Now;
                        var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(earning)));

                        client.SendAsync(message);

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
