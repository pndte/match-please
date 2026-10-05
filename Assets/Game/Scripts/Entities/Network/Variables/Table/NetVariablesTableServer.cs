using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    public sealed class NetVariablesTableServer : NetVariablesTableBase //TODO: Everyone-записи, пришедшие от клиента, сервер не пересылает остальным клиентам, а снимок позднему клиенту их отдаёт — решить, нужна ли пересылка
    {
        private readonly IServerSendersCollection _messageSenders;

        public NetVariablesTableServer(
            Lifetime lifetime,
            NetworkObject networkObject,
            NetEntriesSchema schema,
            IServerSendersCollection messageSenders,
            IOwnership ownership,
            IOwnershipController ownershipController,
            IClientCollection clients,
            IClientPlayerCollection clientPlayers)
            : base(
                lifetime,
                networkObject,
                schema,
                new NetSendGuardFactoryServer(ownership, networkObject),
                new NetReceiveGuardFactoryServer(ownershipController, clients, clientPlayers, networkObject))
        {
            _messageSenders = messageSenders;
            clients.ByIds.View(lifetime, (_, _, client) => SendCurrentStateTo(client));
        }

        protected override void DispatchPropertyUpdate<T>(INetProperty<T> property)
        {
            var sender = _messageSenders.Get<T>();
            sender.Broadcast(HeaderFor(property), property.Value, CurrentRegistration.DeliveryType);
        }

        protected override void DispatchSignalUpdate<T>(INetSignal<T> entry)
        {
            var outgoing = (
                Sender: _messageSenders.Get<T>(),
                Header: HeaderFor(entry),
                Payload: entry.PendingPayload,
                Delivery: CurrentRegistration.DeliveryType);
            
            entry.PendingTarget.Switch(
                outgoing,
                untargeted: static message => message.Sender.Broadcast(message.Header, message.Payload, message.Delivery),
                targeted: static (message, recipient) =>
                    message.Sender.SendToClient(message.Header, message.Payload, message.Delivery, recipient));
        }

        private void SendCurrentStateTo(IClient client)
        {
            if (!NetworkObject.IsSpawned)
                return;

            VisitReplicating(new CurrentStateSender(this, client));
        }

        private sealed class CurrentStateSender : INetSyncVisitor
        {
            private readonly NetVariablesTableServer _table;
            private readonly IClient _recipient;

            public CurrentStateSender(NetVariablesTableServer table, IClient recipient)
            {
                _table = table;
                _recipient = recipient;
            }

            void INetSyncVisitor.VisitProperty<T>(INetProperty<T> property) =>
                _table._messageSenders.Get<T>().SendToClient(
                    _table.HeaderFor(property),
                    property.Value,
                    NetworkDelivery.Reliable,
                    _recipient);

            void INetSyncVisitor.VisitSignal<T>(INetSignal<T> signal)
            {
            }
        }
    }
}
