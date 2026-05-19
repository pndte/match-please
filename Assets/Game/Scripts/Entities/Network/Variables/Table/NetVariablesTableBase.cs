using Bw.Entities.Extensions;
using Bw.Entities.Infrastructure;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    public abstract class NetVariablesTableBase : INetVariablesTable, INetSyncVisitor //TODO: too hard to understand
    {
        public IViewableBiMap<ushort, INetSyncEntry> PropertiesByIndex { get; }

        protected readonly NetworkObject NetworkObject;

        private ushort _counter = 1;

        protected NetRegistryInfo CurrentRegistration { get; private set; }

        protected NetVariablesTableBase(
            Lifetime lifetime,
            NetworkObject networkObject,
            INetPropertyFactory factory,
            IReadonlyOwnership ownership)
        {
            PropertiesByIndex = new ViewableBiMap<ushort, INetSyncEntry>(lifetime);
            NetworkObject = networkObject;
            factory.EntryRegistered.Advise(lifetime, OnNewEntryRegistered);

            void OnNewEntryRegistered(NetRegistryInfo info)
            {
                var entry = info.Entry;
                PropertiesByIndex.Add(_counter++, entry);
                SubscribeDirtyWhenAllowed(lifetime, info, entry, ownership);
            }
        }

        private void SubscribeDirtyWhenAllowed(
            Lifetime lifetime,
            NetRegistryInfo info,
            INetSyncEntry entry,
            IReadonlyOwnership ownership)
        {
            switch (info.Permissions)
            {
                case NetworkPermissions.Owner:
                    BindDirtyReplicationWhileMine(lifetime, info, entry, ownership);
                    break;
                case NetworkPermissions.Everyone:
                    BindDirtyReplication(lifetime, info, entry);
                    break;
                default:
                    SubscribeDirtyForPeerSpecificPermissions(lifetime, info, entry, ownership);
                    break;
            }
        }

        protected abstract void SubscribeDirtyForPeerSpecificPermissions(
            Lifetime lifetime,
            NetRegistryInfo info,
            INetSyncEntry entry,
            IReadonlyOwnership ownership);

        protected void BindDirtyReplicationWhileMine(
            Lifetime lifetime,
            NetRegistryInfo info,
            INetSyncEntry entry,
            IReadonlyOwnership ownership)
        {
            ownership.Mine.View(lifetime, (mineLifetime, mine) =>
            {
                if (!mine) return;
                BindDirtyReplication(mineLifetime, info, entry);
            });
        }

        protected void BindDirtyReplication(Lifetime lifetime, NetRegistryInfo info, INetSyncEntry entry)
        {
            entry.Dirty.AdviseTrue(lifetime, () =>
            {
                CurrentRegistration = info;
                entry.Accept(this);
                entry.Dirty.Value = false;
            });
        }

        public void VisitProperty<T>(INetProperty<T> property) =>
            DispatchPropertyUpdate(property);

        public void VisitSignal<T>(INetSignal<T> entry) =>
            DispatchSignalUpdate(entry);

        protected abstract void DispatchPropertyUpdate<T>(INetProperty<T> property);

        protected abstract void DispatchSignalUpdate<T>(INetSignal<T> entry);

        protected NetworkMessageHeader HeaderFor(INetSyncEntry entry) =>
            new()
            {
                NetworkObjectId = NetworkObject.NetworkObjectId,
                VarId = PropertiesByIndex.Inverse[entry]
            };
    }
}
