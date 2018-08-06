using DataLockActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;

namespace SFA.DAS.Payments.ServiceFabric.AutoActorIdPocClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("---- Initialising Actors ----------");
            Console.WriteLine("Enter C for custom Actor ID generation.");
            var actorIdGenerationType = Console.ReadLine();
            var continueProcessing = true;

            while (continueProcessing)
            {
                try
                {
                    for (long i = 131776765932985901; i < 131776765932985910; i++)
                    {
                        var actorId = actorIdGenerationType == "C" ? new ActorId(i.ToString()) : (ActorId.CreateRandom());
                        var proxy = ActorProxy.Create<IDataLockActor>(actorId, new Uri("fabric:/SFA.DAS.Payments.DataLock/DataLockActor"));
                        var actorService= proxy.GetActorReference();
                        Console.WriteLine(actorService.ActorId.GetPartitionKey());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Console.WriteLine("----Processing completed - Enter Y to Continue Processing ----------");

                var line = Console.ReadLine();
                continueProcessing = line == "Y";
            }


        }
    }
}
