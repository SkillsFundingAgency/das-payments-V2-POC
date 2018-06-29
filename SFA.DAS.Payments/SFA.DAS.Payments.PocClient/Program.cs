using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DataLockActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using SFA.DAS.Payments.Application.Interfaces;

namespace SFA.DAS.Payments.ServiceFabric.PocClient
{
    class Program
    {
        static void Main(string[] args)
        {

            var earnings = TestDataGenerator.TestDataGenerator.CreateEarningsFromLearners();
            //EarningProvider.SetEarnings(earnings);
            //CommitmentProvider.SetCommitments(TestDataGenerator.TestDataGenerator.CreateCommitmentsFromEarnings(earnings));

            Task.Run(() =>
            {
                try
                {
                    for (var i = 0; i < earnings.Count; i++)
                    {
                        var earning = earnings[i];
                        Debug.WriteLine($"------------------------------------ processing earning {i} out of {earnings.Count}------------------------------------");

                        var actor = ActorProxy.Create<IDataLockActor>(new ActorId(earning.Ukprn), new Uri("fabric:/SFA.DAS.Payments.DataLock/DataLockActorService"));

                        actor.ProcessEarning(earning);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }).Wait();

            Debug.WriteLine("*********************************** finished processing earnings ***********************************");
            Console.ReadLine();
        }
    }
}
