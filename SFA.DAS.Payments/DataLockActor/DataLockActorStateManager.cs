using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DataLockActor.Storage;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using SFA.DAS.Payments.Application.Interfaces;

namespace DataLockActor
{
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

            ICommitmentProvider commitmentProvider = new CommitmentProvider();

            var commitments = commitmentProvider.GetCommitments(Ukprn)
                .GroupBy(c => string.Concat(c.Ukprn, "-", c.LearnerReferenceNumber))
                .ToDictionary(c => c.Key, c => c.ToList());

            Debug.WriteLine($"read from provider in {sw.ElapsedMilliseconds.ToString("##,###")}ms");

            sw.Restart();

            var cache = GetLocalCache();

            await cache.Reset();

            foreach (var c in commitments)
            {
                await cache.Add(c.Key, c.Value);
            }

            Debug.WriteLine($"saved in state in {sw.ElapsedMilliseconds.ToString("##,###")}ms");
        }

    }
}