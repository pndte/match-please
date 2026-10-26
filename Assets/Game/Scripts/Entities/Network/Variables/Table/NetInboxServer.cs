using System;

namespace Bw.Entities.Network.Variables
{
    public sealed class NetInboxServer : INetInbox
    {
        private readonly NetEntries _entries;
        private readonly INetReceiveGuard[] _guards;
        private readonly IClientCollection _clients;

        public NetInboxServer(NetEntries entries, NetReceiveGuardFactoryServer guards, IClientCollection clients)
        {
            _entries = entries;
            _guards = GuardsOf(entries, guards);
            _clients = clients;
        }

        void INetInbox.Receive(ulong senderClientId, NetworkMessageHeader header, INetSyncVisitor reader)
        {
            var slot = _entries.At(header.VarId);
            if (!_guards[slot.VarId].TryAdmit(senderClientId))
                return;

            slot.Entry.Accept(reader);
            if (slot.Declaration.Permissions == NetworkPermissions.Everyone)
                slot.Entry.Accept(new EveryoneRelay(WriterOf(senderClientId, header)));
        }

        private IClient WriterOf(ulong senderClientId, NetworkMessageHeader header) =>
            _clients.ByIds.TryGetValue(senderClientId, out var writer)
                ? writer
                : throw new InvalidOperationException(
                    $"Entry {header.VarId.ToString()} of object {header.NetworkObjectId.ToString()} came from client {senderClientId.ToString()}, who is not connected.");

        private static INetReceiveGuard[] GuardsOf(NetEntries entries, NetReceiveGuardFactoryServer guards)
        {
            var slots = entries.Slots;
            var created = new INetReceiveGuard[slots.Count];
            for (var index = 0; index < created.Length; index++)
                created[index] = guards.Create(slots[index].Declaration.Permissions);

            return created;
        }

        private sealed class EveryoneRelay : INetSyncVisitor
        {
            private readonly IClient _writer;

            public EveryoneRelay(IClient writer)
            {
                _writer = writer;
            }

            void INetSyncVisitor.VisitProperty<T>(INetProperty<T> property) =>
                property.Relay(_writer);

            void INetSyncVisitor.VisitSignal<T>(INetSignal<T> signal)
            {
            }
        }
    }
}
