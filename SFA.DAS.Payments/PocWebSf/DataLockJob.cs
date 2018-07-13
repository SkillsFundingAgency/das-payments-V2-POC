using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DataLockActor.Interfaces;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Metric = SFA.DAS.Payments.Domain.Metric;

namespace PocWebSf
{
    public class DataLockJob
    {
        public const string DataLockActorTableStorage = "DataLockActorTableStorage";
        public const string DataLockActorSql = "DataLockActorSql";
        public const string DataLockActorStateManager = "DataLockActorStateManager";

        public void RunDataLock(PerformContext context, string actorType = DataLockActorStateManager)
        {
            var metricBatchId = DateTime.UtcNow.ToString("s") + " " + actorType;
            
            context.SetTextColor(ConsoleTextColor.Yellow);

            context.WriteLine($"New batch {metricBatchId}. Creating earnings... ");

            var earnings = SFA.DAS.Payments.TestDataGenerator.TestDataGenerator.CreateEarningsFromLearners(spreadUkprnAcross: 5);

            context.WriteLine($"{earnings.Count} earnings created.");

            Task.Run(async () =>
            {
                try
                {
                    if (actorType == DataLockActorTableStorage)
                    {
                        context.WriteLine("Resetting table storage...");
                        await SFA.DAS.Payments.TestDataGenerator.TestDataGenerator.ResetAndPopulateTableStorage();
                    }
                    else if (actorType == DataLockActorSql)
                    {
                        context.WriteLine("Resetting SQL server storage...");
                        await SFA.DAS.Payments.TestDataGenerator.TestDataGenerator.ResetAndPopulateSqlStorage();
                    }

                    context.ResetTextColor();

                    var sw1 = Stopwatch.StartNew();
                    var parts = earnings.Select(e => e.Ukprn).Distinct();
                    var stats = new ConcurrentBag<Metric>();

                    Parallel.ForEach(parts, ukprn =>
                    {
                        Task.Run(async () =>
                        {
                            var progress = context.WriteProgressBar("UKPRN " + ukprn);
                            
                            var earningsForUkprn = earnings.Where(e => e.Ukprn == ukprn).ToList();
                            for (var i = 0; i < earningsForUkprn.Count; i++)
                            {
                                try
                                {
                                    var sw2 = Stopwatch.StartNew();
                                    var earning = earningsForUkprn[i];

                                    var actor = ActorProxy.Create<IDataLockActor>(new ActorId(earning.Ukprn), new Uri(string.Format(SFA.DAS.Payments.Domain.Config.Configuration.ActorConnectionString, actorType)));

                                    var proxyTime = sw2.ElapsedTicks;
                                    sw2.Restart();

                                    var metrics = await actor.ProcessEarning(earning);
                                    metrics.OutsideProxy = proxyTime;
                                    metrics.OutsideCall = sw2.ElapsedTicks;
                                    metrics.BatchId = metricBatchId;
                                    metrics.Progress = (double) i / earningsForUkprn.Count;

                                    //context.WriteLine($"{metrics.Actor}-{metrics.Progress:#0%}");
                                    
                                    await SFA.DAS.Payments.TestDataGenerator.TestDataGenerator.WriteMetric(metrics);
                                    stats.Add(metrics);

                                    progress.SetValue(metrics.Progress * 100);
                                }
                                catch(Exception ex)
                                {
                                    context.WriteLine(ex);
                                    break;
                                }
                            }
                        }).Wait();
                    });

                    context.SetTextColor(ConsoleTextColor.Green);
                    context.WriteLine($"Done {stats.Count:#,##0} earnings in {sw1.ElapsedMilliseconds:##,##0}ms");
                    var callerTime = stats.Select(m => m.OutsideCall + m.OutsideProxy).ToList();
                    var calleeTime = stats.Select(m => m.InsideCalc + m.InsideRead + m.InsideWrite).ToList();
                    context.WriteLine($"Total caller time: {callerTime.Sum()/10000:#,##0}ms, average caller time: {callerTime.Average()/10000:#,##0.####}ms");
                    context.WriteLine($"Total callee time: {calleeTime.Sum()/10000:#,##0}ms, average callee time: {calleeTime.Average()/10000:#,##0.####}ms");
                }
                catch (Exception ex)
                {
                    throw;
                }
            }).Wait();
        }
    }
}