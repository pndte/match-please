using System;
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
            IClientSendersCollection messageSenders,
            INetPropertyFactory factory,
            IReadonlyOwnership ownership)
            : base(lifetime, networkObject, factory, ownership)
        {
            _messageSenders = messageSenders;
        }

        protected override void SubscribeDirtyForPeerSpecificPermissions(
            Lifetime lifetime,
            NetRegistryInfo info,
            INetSyncEntry entry,
            IReadonlyOwnership ownership)
        {
            switch (info.Permissions)
            {
                case NetworkPermissions.Client:
                    BindDirtyReplicationWhileMine(lifetime, info, entry, ownership);
                    break;
                case NetworkPermissions.Server:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(info.Permissions),
                        info.Permissions,
                        "Unknown NetworkPermissions value.");
            }
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
    }
}
