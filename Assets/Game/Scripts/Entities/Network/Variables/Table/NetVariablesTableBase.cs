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
        internal const ushort SchemaHashVarId = 0;

        protected readonly NetworkObject NetworkObject;

        private readonly ViewableBiMap<ushort, INetSyncEntry> _entriesByVarId;
        private readonly Dictionary<NetEntryDeclaration, INetSyncEntry> _entriesByDeclaration = new();
        private readonly List<INetSyncEntry> _entries = new();
        private readonly HashSet<INetSyncEntry> _replicating = new();
        private readonly uint _schemaHash;

        protected NetRegistryInfo CurrentRegistration { get; private set; }

        private protected NetVariablesTableBase(
            Lifetime lifetime,
            NetworkObject networkObject, //TODO: заменить на свою абстракцию
            NetEntriesSchema schema,
            INetSendGuardFactory sendGuards)
        {
            NetworkObject = networkObject;
            _schemaHash = schema.Hash;
            _entriesByVarId = new ViewableBiMap<ushort, INetSyncEntry>(lifetime);

            ushort varId = SchemaHashVarId + 1;
            foreach (var declaration in schema.Declarations)
                AddEntry(lifetime, varId++, declaration, sendGuards.Create(declaration.Permissions));
        }

        public INetSignal<T> Get<T>(NetSignalDeclaration<T> declaration) =>
            (INetSignal<T>)EntryFor(declaration);

        public INetProperty<T> Get<T>(NetPropertyDeclaration<T> declaration) =>
            (INetProperty<T>)EntryFor(declaration);

        INetSyncEntry INetEntries.EntryWritableBy(ulong senderClientId, ushort varId)
        {
            if (!_entriesByVarId.TryGetValue(varId, out var entry))
                throw new InvalidOperationException(
                    $"No net entry with id {varId.ToString()} on object {NetworkObject.NetworkObjectId.ToString()}:{NetworkObject.name}, sender: client {senderClientId.ToString()}.");

            CheckWriter(senderClientId, entry);
            return entry;
        }

        void INetEntries.CheckSchemaOf(ulong senderClientId, uint schemaHash)
        {
            if (schemaHash != _schemaHash) //TODO: сервер должен отключать клиента с другой схемой (играть он всё равно не сможет)
                throw new InvalidOperationException(
                    $"Net entries schema of object {NetworkObject.NetworkObjectId.ToString()}:{NetworkObject.name} differs between this peer and sender {senderClientId.ToString()}: " +
                    $"{schemaHash.ToString("X8")} there, {_schemaHash.ToString("X8")} here. The peers declare other entries or in another order.");
        }

        private protected abstract void CheckWriter(ulong senderClientId, INetSyncEntry entry);

        private protected void VisitReplicating(INetSyncVisitor visitor)
        {
            for (var index = 0; index < _entries.Count; index++)
            {
                var entry = _entries[index];
                if (_replicating.Contains(entry))
                    entry.Accept(visitor);
            }
        }

        private void AddEntry(Lifetime lifetime, ushort varId, NetEntryDeclaration declaration, INetSendGuard sendGuard)
        {
            var entry = declaration.Create(sendGuard);
            var info = new NetRegistryInfo(entry, declaration.DeliveryType, declaration.Permissions);

            _entriesByVarId.Add(varId, entry);
            _entriesByDeclaration.Add(declaration, entry);
            _entries.Add(entry);
            sendGuard.WhenOpen(lifetime, openLifetime =>
            {
                _replicating.Add(entry);
                openLifetime.OnTermination(() => _replicating.Remove(entry));
                BindDirtyReplication(openLifetime, info, entry);
            });
        }

        private protected INetSyncEntry EntryFor(NetEntryDeclaration declaration) =>
            _entriesByDeclaration.TryGetValue(declaration, out var entry)
                ? entry
                : throw new InvalidOperationException(
                    $"The declaration is not in the schema of object {NetworkObject.NetworkObjectId.ToString()}, it was declared for another net object.");

        private void BindDirtyReplication(Lifetime lifetime, NetRegistryInfo info, INetSyncEntry entry)
        {
            entry.Dirty.AdviseTrue(lifetime, () =>
            {
                entry.Dirty.Value = false;
                if (!NetworkObject.IsSpawned) //TODO: свойства, записанные до спавна, не дойдут до уже подключённых клиентов (поздним их пришлёт снимок состояния)
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
