using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLockActor;
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

            var earnings = TestDataGenerator.TestDataGenerator.CreateEarningsFromLearners(spreadUkprnAcross: 5);
            //EarningProvider.SetEarnings(earnings);
            //CommitmentProvider.SetCommitments(TestDataGenerator.TestDataGenerator.CreateCommitmentsFromEarnings(earnings));
            Debug.WriteLine("#,time inside,incl read,incl calc,incl output,incl write,time outside,incl getproxy,incl call");
            Task.Run(async () =>
            {
                try
                {
                    //await TestDataGenerator.TestDataGenerator.ResetAndPopulateTableStorage();
                    await TestDataGenerator.TestDataGenerator.ResetAndPopulateSqlStorage();

                    var sw1 = Stopwatch.StartNew();
                    //var avg = new List<long>();
                    var parts = earnings.Select(e => e.Ukprn).Distinct();
                    Parallel.ForEach(parts, async ukprn =>
                    {
                        var earningsForUkprn = earnings.Where(e => e.Ukprn == ukprn).ToList();
                        for (var i = 0; i < earningsForUkprn.Count; i++)
                        {
                            var sw2 = Stopwatch.StartNew();
                            var earning = earningsForUkprn[i];
                            var actor = ActorProxy.Create<IDataLockActor>(new ActorId(earning.Ukprn), new Uri("fabric:/SFA.DAS.Payments.DataLock/DataLockActorSql"));

                            var proxyTime = sw2.ElapsedTicks;
                            sw2.Restart();

                            var metrics = await actor.ProcessEarning(earning);
                            metrics.OutsideProxy = proxyTime;
                            metrics.OutsideCall = sw2.ElapsedTicks;

                            Debug.WriteLine($"{metrics.Actor}: #{i},outside,{metrics.OutsideProxy},{metrics.OutsideCall}, inside, {metrics.InsideRead},{metrics.InsideCalc},{metrics.InsideWrite}");
                        }

                    });

                    Debug.WriteLine($"Done full list in {sw1.ElapsedMilliseconds:##,##0}ms");
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
