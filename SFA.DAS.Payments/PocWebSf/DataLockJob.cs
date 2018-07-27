using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataLockActor.Interfaces;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using SFA.DAS.Payments.Domain.Config;
using SFA.DAS.Payments.TestDataGenerator;
using Metric = SFA.DAS.Payments.Domain.Metric;

namespace PocWebSf
{
    public class ConfigEntity : TableEntity
    {
        public string Map { get; set; }
    }

    public class DataLockJob
    {
        public const string DataLockActorTableStorage = "DataLockActorTableStorage";
        public const string DataLockActorSql = "DataLockActorSql";
        public const string DataLockActorStateManager = "DataLockActorStateManager";

        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(5)]
        public void RunDataLock(PerformContext context, IJobCancellationToken cancellationToken, string actorType = DataLockActorStateManager)
        {
            var metricBatchId = DateTime.UtcNow.ToString("s") + " " + actorType;
            
            context.SetTextColor(ConsoleTextColor.Yellow);

            context.WriteLine($"New batch {metricBatchId}. Creating earnings... ");

            var earnings = SFA.DAS.Payments.TestDataGenerator.TestDataGenerator.CreateEarningsFromLearners();

            context.WriteLine($"{earnings.Count} earnings created.");

            Task.Run(async () =>
            {
                try
                {
                    var table = CloudStorageAccount.Parse(Configuration.TableStorageServerConnectionString).CreateCloudTableClient().GetTableReference("Config");
                    await table.CreateIfNotExistsAsync();
                    var tableOperation = TableOperation.Retrieve<ConfigEntity>("LOCAL", "Config");
                    var json = await table.ExecuteAsync(tableOperation);
                    ConcurrentDictionary<long, long> map;
                    if (json.HttpStatusCode == (int) HttpStatusCode.OK && json.Result != null)
                        map = JsonConvert.DeserializeObject<ConcurrentDictionary<long, long>>(((ConfigEntity)json.Result).Map);
                    else
                        map = new ConcurrentDictionary<long, long>();

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
                            long lastMetricsWrite = 0;

                            for (var i = 0; i < earningsForUkprn.Count; i++)
                            {
                                try
                                {
                                    var sw2 = Stopwatch.StartNew();
                                    var earning = earningsForUkprn[i];

                                    //long actorGuid;
                                    //ActorId actorId;

                                    //if (map.TryGetValue(earning.Ukprn, out actorGuid))
                                    //{
                                    //    actorId = new ActorId(actorGuid);
                                    //}
                                    //else
                                    //{
                                    //    actorId = ActorId.CreateRandom();
                                    //    actorGuid = actorId.GetLongId();
                                    //    map.TryAdd(earning.Ukprn, actorGuid);
                                    //    await table.ExecuteAsync(TableOperation.InsertOrReplace(new ConfigEntity
                                    //    {
                                    //        PartitionKey = "LOCAL",
                                    //        RowKey = "Config",
                                    //        Map = JsonConvert.SerializeObject(map)
                                    //    }));
                                    //}

                                    var actor = ActorProxy.Create<IDataLockActor>(new ActorId(ukprn), new Uri(string.Format(SFA.DAS.Payments.Domain.Config.Configuration.ActorConnectionString, actorType)));

                                    var proxyTime = sw2.ElapsedTicks;
                                    sw2.Restart();

                                    var metrics = await actor.ProcessEarning(earning);
                                    metrics.OutsideProxy = proxyTime;
                                    metrics.OutsideCall = sw2.ElapsedTicks;
                                    metrics.BatchId = metricBatchId;
                                    metrics.Progress = (double) i / earningsForUkprn.Count;
                                    metrics.WriteMetrics = lastMetricsWrite;

                                    //context.WriteLine($"{metrics.Actor}-{metrics.Progress:#0%}");
                                    sw2.Restart();

                                    await SFA.DAS.Payments.TestDataGenerator.TestDataGenerator.WriteMetric(metrics);
                                    stats.Add(metrics);

                                    try
                                    {
                                        progress.SetValue(metrics.Progress * 100);
                                    }
                                    catch
                                    {
                                    }

                                    lastMetricsWrite = sw2.ElapsedTicks;

                                    //cancellationToken.ThrowIfCancellationRequested();
                                }
                                catch(Exception ex)
                                {
                                    context.WriteLine(ex);
                                    break;
                                }
                            }
                        }, cancellationToken.ShutdownToken).Wait();
                    //}).Wait();
                    });

                    context.SetTextColor(ConsoleTextColor.Green);
                    context.WriteLine($"Done {stats.Count:#,##0} earnings in {sw1.ElapsedMilliseconds:##,##0}ms");
                    var callerTime = stats.Select(m => m.OutsideCall + m.OutsideProxy).ToList();
                    var calleeTime = stats.Select(m => m.InsideCalc + m.InsideRead + m.InsideWrite).ToList();
                    context.WriteLine($"Total caller time: {callerTime.Sum()/10000:#,##0}ms, average caller time: {callerTime.Average()/10000:#,##0.####}ms");
                    context.WriteLine($"Total callee time: {calleeTime.Sum()/10000:#,##0}ms, average callee time: {calleeTime.Average()/10000:#,##0.####}ms");
                }
                catch (Exception)
                {
                    //BackgroundJob.Delete(context.BackgroundJob.Id);
                    throw;
                }
            }).Wait();
        }
    }
}