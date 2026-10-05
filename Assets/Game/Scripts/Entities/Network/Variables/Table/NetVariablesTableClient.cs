using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    public sealed class NetVariablesTableClient : NetVariablesTableBase
    {
        private readonly IClientSendersCollection _messageSenders;

        public NetVariablesTableClient(
            Lifetime lifetime,
            NetworkObject networkObject,
            NetEntriesSchema schema,
            IClientSendersCollection messageSenders,
            IOwnership ownership)
            : base(lifetime, networkObject, schema, new NetSendGuardFactoryClient(ownership, networkObject))
        {
            _messageSenders = messageSenders;
        }

        protected override void DispatchPropertyUpdate<T>(INetProperty<T> property)
        {
            var sender = _messageSenders.Get<T>();
            sender.SendToServer(HeaderFor(property), property.Value, CurrentRegistration.DeliveryType);
        }

        protected override void DispatchSignalUpdate<T>(INetSignal<T> entry)
        {
            var sender = _messageSenders.Get<T>();
            sender.SendToServer(HeaderFor(entry), entry.PendingPayload, CurrentRegistration.DeliveryType);
        }

        private protected override void CheckWriter(ulong senderClientId, INetSyncEntry entry)
        { //TODO: нарушенный принцип Лисков
        }
    }
}
