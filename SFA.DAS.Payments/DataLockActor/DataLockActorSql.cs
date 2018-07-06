using DataLockActor.Storage;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace DataLockActor
{
    [ActorService(Name = "DataLockActorSql")]
    internal class DataLockActorSql : DataLockActorBase
    {
        public DataLockActorSql(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override ILocalCommitmentCache GetLocalCache()
        {
            return new SqlStorage();
        }
    }
}
