using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DataLockActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace SFA.DAS.Payments.ServiceFabric.PocClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var earnings = TestDataGenerator.TestDataGenerator.CreateEarningsFromLearners();
            bool terminate = false;
            while (!terminate)
            {
                //var actorType = "DataLockActorTableStorage";
                //var actorType = "DataLockActorSql";
                var actorType = "DataLockActorStateManager";
                var metricBatchId = DateTime.UtcNow.ToString("s") + " " + actorType;

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
                                try
                                {
                                    var sw2 = Stopwatch.StartNew();
                                    var earning = earningsForUkprn[i];
                                    //var actor = ActorProxy.Create<IDataLockActor>(new ActorId(earning.Ukprn), new Uri($"fabric:/sfa-das-payments-poc.ukwest.cloudapp.azure.com:19000/SFA.DAS.Payments.DataLock/{actorType}"));

                                    var actor = ActorProxy.Create<IDataLockActor>(new ActorId(earning.Ukprn), new Uri($"fabric:/SFA.DAS.Payments.DataLock/{actorType}"));

                                    var proxyTime = sw2.ElapsedTicks;
                                    sw2.Restart();

                                    var metrics = await actor.ProcessEarning(earning);
                                    metrics.OutsideProxy = proxyTime;
                                    metrics.OutsideCall = sw2.ElapsedTicks;
                                    metrics.BatchId = metricBatchId;

                                    //Debug.WriteLine($"{metrics.Actor}: #{i},outside,{metrics.OutsideProxy},{metrics.OutsideCall}, inside, {metrics.InsideRead},{metrics.InsideCalc},{metrics.InsideWrite}");
                                    Debug.WriteLine($"{metrics.Actor}-{(double) i / earningsForUkprn.Count:#0%}");
                                    await TestDataGenerator.TestDataGenerator.WriteMetric(metrics);
                                }
                                catch
                                {
                                }
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
                var line = Console.ReadLine();
                terminate = line == "Y";
            }
        }
    }
}
