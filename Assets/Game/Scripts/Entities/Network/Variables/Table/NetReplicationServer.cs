using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    public sealed class NetReplicationServer
    {
        private readonly HashSet<NetEntrySlot> _open = new();
        private readonly Dictionary<NetEntrySlot, ToClients> _dispatches = new();
        private readonly NetEntries _entries;
        private readonly INetworkLifetimedObject _networkObject;
        private readonly IWorldStep _worldStep;
        private readonly IServerSendersCollection _senders;
        private readonly IClientCollection _clients;

        private Lifetime _announcedLifetime = Lifetime.Terminated;

        public NetReplicationServer(
            Lifetime lifetime,
            NetEntries entries,
            INetworkLifetimedObject networkObject,
            IWorldStep worldStep,
            IServerSendersCollection senders,
            IClientCollection clients)
        {
            _entries = entries;
            _networkObject = networkObject;
            _worldStep = worldStep;
            _senders = senders;
            _clients = clients;

            foreach (var slot in entries.Slots)
            {
                _dispatches.Add(slot, new ToClients(senders, clients, networkObject, slot));
                slot.SendGuard.WhenOpen(lifetime, openLifetime =>
                {
                    openLifetime.Bracket(() => _open.Add(slot), () => _open.Remove(slot));
                    slot.Entry.Dirty.AdviseTrue(openLifetime, () => Settle(slot));
                });
            }

            networkObject.SpawnedLifetime.WhenAlive(lifetime, AnnounceOnPublish);
        }

        private void AnnounceOnPublish(Lifetime spawnedLifetime)
        {
            spawnedLifetime.OnTermination(_entries.Slots.ForgetPending);
            var waiting = spawnedLifetime.CreateNested();
            _worldStep.Stepped(TickPhase.Publish).Advise(waiting.Lifetime, _ =>
            {
                waiting.Terminate();
                Announce(spawnedLifetime);
            });
        }

        private void Announce(Lifetime announcedLifetime)
        {
            _announcedLifetime = announcedLifetime;
            foreach (var slot in _entries.Slots)
                if (_open.Contains(slot) && slot.Entry.Dirty.Value)
                    Settle(slot);

            _clients.ByIds.View(announcedLifetime, (_, _, client) => SendStateTo(client));
        }

        private void Settle(NetEntrySlot slot)
        {
            if (_announcedLifetime.IsAlive)
            {
                slot.Entry.Dirty.Value = false;
                slot.Entry.Accept(_dispatches[slot]);
            }
            else if (!_networkObject.SpawnedLifetime.Value.IsAlive)
                slot.Entry.Dirty.Value = false;
        }

        private void SendStateTo(IClient client)
        {
            foreach (var slot in _open)
                slot.Entry.Accept(new StateTo(_senders, slot.HeaderOn(_networkObject.Id), client));
        }

        private sealed class ToClients : INetSyncVisitor
        {
            private NetworkMessageHeader Header => _slot.HeaderOn(_networkObject.Id);

            private readonly IServerSendersCollection _senders;
            private readonly IClientCollection _clients;
            private readonly INetworkLifetimedObject _networkObject;
            private readonly NetEntrySlot _slot;

            public ToClients(
                IServerSendersCollection senders,
                IClientCollection clients,
                INetworkLifetimedObject networkObject,
                NetEntrySlot slot)
            {
                _senders = senders;
                _clients = clients;
                _networkObject = networkObject;
                _slot = slot;
            }

            void INetSyncVisitor.VisitProperty<T>(INetProperty<T> property) =>
                property.PendingTarget.Switch(
                    (Sender: _senders.Get<T>(), Clients: _clients, Header, Value: property.Value, Delivery: _slot.Declaration.DeliveryType),
                    everyone: static message => message.Sender.Broadcast(message.Header, message.Value, message.Delivery),
                    except: static (message, writer) =>
                    {
                        foreach (var client in message.Clients.ByIds.Values)
                            if (client != writer)
                                message.Sender.SendToClient(message.Header, message.Value, message.Delivery, client);
                    });

            void INetSyncVisitor.VisitSignal<T>(INetSignal<T> signal) =>
                signal.PendingTarget.Switch(
                    (Sender: _senders.Get<T>(), Header, Payload: signal.PendingPayload, Delivery: _slot.Declaration.DeliveryType),
                    untargeted: static message => message.Sender.Broadcast(message.Header, message.Payload, message.Delivery),
                    targeted: static (message, recipient) =>
                        message.Sender.SendToClient(message.Header, message.Payload, message.Delivery, recipient));
        }

        private sealed class StateTo : INetSyncVisitor
        {
            private readonly IServerSendersCollection _senders;
            private readonly NetworkMessageHeader _header;
            private readonly IClient _recipient;

            public StateTo(IServerSendersCollection senders, NetworkMessageHeader header, IClient recipient)
            {
                _senders = senders;
                _header = header;
                _recipient = recipient;
            }

            void INetSyncVisitor.VisitProperty<T>(INetProperty<T> property) =>
                _senders.Get<T>().SendToClient(_header, property.Value, NetworkDelivery.Reliable, _recipient);

            void INetSyncVisitor.VisitSignal<T>(INetSignal<T> signal)
            {
            }
        }
    }
}
