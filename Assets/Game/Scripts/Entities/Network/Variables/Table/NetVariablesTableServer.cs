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
            var sender = _messageSenders.Get<T>();
            var header = HeaderFor(entry);
            var payload = entry.PendingPayload;
            var delivery = CurrentRegistration.DeliveryType;
            
            entry.PendingTarget.Switch(
                untargeted: () => sender.Broadcast(header, payload, delivery),
                targeted: recipient => sender.SendToClient(header, payload, delivery, recipient));
        }
    }
}
