using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

[assembly: FabricTransportActorRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]
namespace DataLockActor.Interfaces
{
    public interface IDataLockActor : IActor
    {
        Task<int> GetCountAsync(CancellationToken cancellationToken);

        Task SetCountAsync(int count, CancellationToken cancellationToken);
    }
}
