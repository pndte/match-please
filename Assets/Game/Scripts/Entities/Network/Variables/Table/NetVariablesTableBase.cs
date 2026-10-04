using System;
using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Entities.Infrastructure;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    public abstract class NetVariablesTableBase : INetEntries, INetSyncVisitor //TODO: too hard to understand
    {
        protected readonly NetworkObject NetworkObject;

        private readonly ViewableBiMap<ushort, INetSyncEntry> _entriesByVarId;
        private readonly Dictionary<NetEntryDeclaration, INetSyncEntry> _entriesByDeclaration = new();

        protected NetRegistryInfo CurrentRegistration { get; private set; }

        private protected NetVariablesTableBase(
            Lifetime lifetime,
            NetworkObject networkObject, //TODO: заменить на свою абстракцию
            NetEntriesSchema schema,
            INetSendGuardFactory sendGuards)
        {
            NetworkObject = networkObject;
            _entriesByVarId = new ViewableBiMap<ushort, INetSyncEntry>(lifetime);

            ushort varId = 1;
            foreach (var declaration in schema.Declarations)
                AddEntry(lifetime, varId++, declaration, sendGuards.Create(declaration.Permissions));
        }

        public INetSignal<T> Get<T>(NetSignalDeclaration<T> declaration) =>
            (INetSignal<T>)EntryFor(declaration);

        public INetProperty<T> Get<T>(NetPropertyDeclaration<T> declaration) =>
            (INetProperty<T>)EntryFor(declaration);

        bool INetEntries.TryGetEntry(ushort varId, out INetSyncEntry entry) =>
            _entriesByVarId.TryGetValue(varId, out entry);

        private void AddEntry(Lifetime lifetime, ushort varId, NetEntryDeclaration declaration, INetSendGuard sendGuard)
        {
            var entry = declaration.Create(sendGuard);
            var info = new NetRegistryInfo(entry, declaration.DeliveryType, declaration.Permissions);

            _entriesByVarId.Add(varId, entry);
            _entriesByDeclaration.Add(declaration, entry);
            sendGuard.WhenOpen(lifetime, openLifetime => BindDirtyReplication(openLifetime, info, entry));
        }

        private INetSyncEntry EntryFor(NetEntryDeclaration declaration) =>
            _entriesByDeclaration.TryGetValue(declaration, out var entry)
                ? entry
                : throw new InvalidOperationException(
                    $"The declaration is not in the schema of object {NetworkObject.NetworkObjectId}, it was declared for another net object.");

        private void BindDirtyReplication(Lifetime lifetime, NetRegistryInfo info, INetSyncEntry entry)
        {
            entry.Dirty.AdviseTrue(lifetime, () =>
            {
                entry.Dirty.Value = false;
                if (!NetworkObject.IsSpawned)
                    return;

                CurrentRegistration = info;
                entry.Accept(this);
            });
        }

        void INetSyncVisitor.VisitProperty<T>(INetProperty<T> property) =>
            DispatchPropertyUpdate(property);

        void INetSyncVisitor.VisitSignal<T>(INetSignal<T> entry) =>
            DispatchSignalUpdate(entry);

        protected abstract void DispatchPropertyUpdate<T>(INetProperty<T> property);

        protected abstract void DispatchSignalUpdate<T>(INetSignal<T> entry);

        protected NetworkMessageHeader HeaderFor(INetSyncEntry entry)
        {
            if (!_entriesByVarId.TryGetLeft(entry, out var varId))
                throw new InvalidOperationException("Net sync entry is not registered in this variables table.");

            return new NetworkMessageHeader
            {
                NetworkObjectId = NetworkObject.NetworkObjectId,
                VarId = varId
            };
        }
    }

    public struct NetRegistryInfo
    {
        public INetSyncEntry Entry;
        public NetworkDelivery DeliveryType;
        public NetworkPermissions Permissions;

        public NetRegistryInfo(INetSyncEntry entry, NetworkDelivery deliveryType, NetworkPermissions permissions)
        {
            DeliveryType = deliveryType;
            Permissions = permissions;
            Entry = entry;
        }
    }
}
