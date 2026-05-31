using System;
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
            IServerSendersCollection messageSenders,
            INetPropertyFactory factory,
            IOwnership ownership)
            : base(lifetime, networkObject, factory, ownership)
        {
            _messageSenders = messageSenders;
        }

        protected override void SubscribeDirtyForPeerSpecificPermissions(
            Lifetime lifetime,
            NetRegistryInfo info,
            INetSyncEntry entry,
            IOwnership ownership)
        {
            switch (info.Permissions)
            {
                case NetworkPermissions.Server:
                    BindDirtyReplication(lifetime, info, entry);
                    break;
                case NetworkPermissions.Client:
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
            sender.Dispatch(HeaderFor(property), property.Value, CurrentRegistration.DeliveryType);
        }

        protected override void DispatchSignalUpdate<T>(INetSignal<T> entry)
        {
            var sender = _messageSenders.Get<T>();
            sender.Dispatch(HeaderFor(entry), entry.PendingPayload, CurrentRegistration.DeliveryType);
        }
    }
}
