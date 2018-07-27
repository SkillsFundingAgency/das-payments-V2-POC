using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataLockActor.Storage;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using SFA.DAS.Payments.Application.Interfaces;
using SFA.DAS.Payments.Domain.Config;
using SFA.DAS.Payments.TestDataGenerator;

namespace DataLockActor
{
    [StatePersistence(StatePersistence.Volatile)]
    [ActorService(Name = "DataLockActorStateManager")]
    internal class DataLockActorStateManager : DataLockActorBase
    {
        public DataLockActorStateManager(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override ILocalCommitmentCache GetLocalCache()
        {
            return new StateManagerStorage(StateManager);
        }

        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // load commitments
            var sw = Stopwatch.StartNew();

            var stateNames = await StateManager.GetStateNamesAsync();
            if (stateNames != null && stateNames.Any())
            {
                TestDataGenerator.Log("DataLockActorStateManager", $"skiping initialisation, state has {stateNames.Count()} keys");
                Debug.WriteLine($"?????????????????????????????????? skiping initialisation, state has {stateNames.Count()} keys ?????????????????????????????????? ");
                return;
            }

            TestDataGenerator.Log("DataLockActorStateManager", $" initialising, state has no keys keys");
            Debug.WriteLine($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! initialising, state has no keys keys !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ");

            var cache = GetLocalCache();

            ICommitmentProvider commitmentProvider = new CommitmentProvider();

            //var table = CloudStorageAccount.Parse(Configuration.TableStorageServerConnectionString).CreateCloudTableClient().GetTableReference("Config");
            //var tableOperation = TableOperation.Retrieve<ConfigEntity>("LOCAL", "Config");
            //var json = await table.ExecuteAsync(tableOperation);
            //var map = JsonConvert.DeserializeObject<ConcurrentDictionary<long, long>>(((ConfigEntity) json.Result).Map);
            //Ukprn = map.Single(kvp => kvp.Value == Ukprn).Key;

            var commitments = commitmentProvider.GetCommitments(Ukprn)
                .GroupBy(c => string.Concat(c.Ukprn, "-", c.LearnerReferenceNumber))
                .ToDictionary(c => c.Key, c => c.ToList());

            Debug.WriteLine($"read from provider in {sw.ElapsedMilliseconds.ToString("##,###")}ms");

            sw.Restart();


            await cache.Reset();

            foreach (var c in commitments)
            {
                await cache.Add(c.Key, c.Value);
            }

            Debug.WriteLine($"saved in state in {sw.ElapsedMilliseconds.ToString("##,###")}ms");
            TestDataGenerator.Log("DataLockActorStateManager", $" saved in state in {sw.ElapsedMilliseconds.ToString("##,###")}ms");
        }
    }
    public class ConfigEntity : TableEntity
    {
        public string Map { get; set; }
    }
}