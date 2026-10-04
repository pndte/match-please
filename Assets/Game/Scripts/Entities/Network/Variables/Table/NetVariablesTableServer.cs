using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    public sealed class NetVariablesTableServer : NetVariablesTableBase
    {
        private readonly IServerSendersCollection _messageSenders;

        public NetVariablesTableServer(
            Lifetime lifetime,
            NetworkObject networkObject,
            NetEntriesSchema schema,
            IServerSendersCollection messageSenders,
            IOwnership ownership)
            : base(lifetime, networkObject, schema, new NetSendGuardFactoryServer(ownership, networkObject))
        {
            _messageSenders = messageSenders;
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
    }
}
