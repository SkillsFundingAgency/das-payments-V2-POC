using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Persistence.ServiceFabric;

namespace DataLockActorProxyService
{
    public class StatefulEndpointCommunicationListener :
        ICommunicationListener
    {
        private readonly IReliableStateManager stateManager;
        private IEndpointInstance endpointInstance;
        private EndpointConfiguration endpointConfiguration;

        public StatefulEndpointCommunicationListener(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var endpointName = "sfa-das-payments-datalockproxyservice";

            endpointConfiguration = new EndpointConfiguration(endpointName);
            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(
                type => type.Namespace == "SFA.DAS.Payments.DataLock.Messages.Commands");
            conventions.DefiningEventsAs(
                type => type.Namespace == "SFA.DAS.Payments.DataLock.Messages.Events");
            conventions.DefiningMessagesAs(
                type => type.Namespace == "SFA.DAS.Payments.DataLock.Messages");
            // configure endpoint with state manager dependency
            //var persistence = endpointConfiguration.UsePersistence<AzureStoragePersistence>();
            //persistence.ConnectionString("UseDevelopmentStorage=true");
            var persistence = endpointConfiguration.UsePersistence<ServiceFabricPersistence>();
            persistence.StateManager(stateManager);
            endpointConfiguration.DisableFeature<TimeoutManager>();
            endpointConfiguration.DisableFeature<MessageDrivenSubscriptions>();
            var transport = endpointConfiguration.UseTransport<AzureStorageQueueTransport>();
            transport.ConnectionString("UseDevelopmentStorage=true");
            transport.BatchSize(1);
            transport.DegreeOfReceiveParallelism(1);
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.EnableInstallers();
            return Task.FromResult(endpointName);
        }

        public async Task RunAsync()
        {
            try
            {
                endpointInstance = await Endpoint.Start(endpointConfiguration)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error starting endpoint. Error: {ex.Message}. Ex: {ex}");
                throw;
            }
        } 

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return endpointInstance.Stop();
        }

        public void Abort()
        {
            CloseAsync(CancellationToken.None);
        }
    }
}