using System;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

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

            const int messageSize = 1000;

            try
            {
                while (!exit)
                {
                    var learners = TestDataGenerator.TestDataGenerator.CreateEarningsFromLearners();

                    var current = learners.Take(messageSize).ToList();

                    while (current.Any())
                    {
                        var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(current)));

                        client.SendAsync(message);

                        current = learners.Skip(messageSize).Take(messageSize).ToList();
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
