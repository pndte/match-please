using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    public sealed class NetReplicationClient
    {
        private readonly INetworkLifetimedObject _networkObject;

        public NetReplicationClient(
            Lifetime lifetime,
            NetEntries entries,
            INetworkLifetimedObject networkObject,
            IClientSendersCollection senders)
        {
            _networkObject = networkObject;

            foreach (var slot in entries.Slots)
            {
                var dispatch = new ToServer(senders, networkObject, slot);
                slot.SendGuard.WhenOpen(lifetime, openLifetime => slot.Entry.Dirty.AdviseTrue(openLifetime, () => Send(slot, dispatch)));
            }
        }

        private void Send(NetEntrySlot slot, ToServer dispatch)
        {
            slot.Entry.Dirty.Value = false;
            if (_networkObject.SpawnedLifetime.Value.IsAlive)
                slot.Entry.Accept(dispatch);
        }

        private sealed class ToServer : INetSyncVisitor
        {
            private NetworkMessageHeader Header => _slot.HeaderOn(_networkObject.Id);

            private readonly IClientSendersCollection _senders;
            private readonly INetworkLifetimedObject _networkObject;
            private readonly NetEntrySlot _slot;

            public ToServer(IClientSendersCollection senders, INetworkLifetimedObject networkObject, NetEntrySlot slot)
            {
                _senders = senders;
                _networkObject = networkObject;
                _slot = slot;
            }

            void INetSyncVisitor.VisitProperty<T>(INetProperty<T> property) =>
                _senders.Get<T>().SendToServer(Header, property.Value, _slot.Declaration.DeliveryType);

            void INetSyncVisitor.VisitSignal<T>(INetSignal<T> signal) =>
                _senders.Get<T>().SendToServer(Header, signal.PendingPayload, _slot.Declaration.DeliveryType);
        }
    }
}
