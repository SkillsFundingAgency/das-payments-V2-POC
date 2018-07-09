using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DataLockActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Data.Collections;
using NServiceBus;
using NServiceBus.Persistence.ServiceFabric;
using SFA.DAS.Payments.DataLock.Messages;
using SFA.DAS.Payments.DataLock.Messages.Commands;

namespace DataLockActorProxyService.Handlers
{
    public class ProcessEarningHandler : IHandleMessages<ProcessEarning>
    {
        public async Task Handle(ProcessEarning message, IMessageHandlerContext context)
        {
            try
            {
                var session = context.SynchronizedStorageSession.ServiceFabricSession();
                var stateManager = session.StateManager;
                IReliableDictionary<string, ActorId> dictionary;
                using (var transaction = stateManager.CreateTransaction())
                {
                    dictionary = await stateManager.GetOrAddAsync<IReliableDictionary<string, ActorId>>(transaction, "datalockactors")
                        .ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                };
                ActorId actorId;
                using (var transaction = stateManager.CreateTransaction())
                {
                    actorId = await dictionary
                        .GetOrAddAsync(transaction, message.Earning.Ukprn.ToString(), ukprn => ActorId.CreateRandom())
                        .ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                }

                var actor = ActorProxy.Create<IDataLockActor>(actorId, new Uri("fabric:/SFA.DAS.Payments.DataLock/DataLockActorService"));
                await actor.ProcessEarning(message.Earning).ConfigureAwait(false);
                //TODO: publish result

            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error: {ex.Message}, Ex: {ex}");
                throw;
            }
        }
    }
}