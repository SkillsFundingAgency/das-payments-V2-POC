using DataLockActor.Storage;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace DataLockActor
{
    [ActorService(Name = "DataLockActorTableStorage")]
    internal class DataLockActorTableStorage : DataLockActorBase
    {
        public DataLockActorTableStorage(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override ILocalCommitmentCache GetLocalCache()
        {
            return new TableStorage();
        }
    }
}
