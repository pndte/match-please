using Bw.Entities.Extensions;
using Bw.Entities.Infrastructure;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    public interface INetVariablesTable
    {
        IViewableBiMap<ushort, INetSyncEntry> PropertiesByIndex { get; }
    }

    public class NetVariablesTable : INetVariablesTable, INetSyncVisitor
    {
        public IViewableBiMap<ushort, INetSyncEntry> PropertiesByIndex { get; }

        private readonly NetworkObject _networkObject;
        private readonly IMessageSenders _messageSenders;

        private ushort _counter = 1;
        private NetRegistryInfo _currentRegistration;

        public NetVariablesTable(
            Lifetime lifetime,
            NetworkObject networkObject,
            IMessageSenders senders,
            INetPropertyFactory factory,
            IRuntimeSettings runtimeSettings)
        {
            PropertiesByIndex = new ViewableBiMap<ushort, INetSyncEntry>(lifetime);
            _networkObject = networkObject;
            _messageSenders = senders;
            factory.EntryRegistered.Advise(lifetime, OnNewEntryRegistered);
            return;

            void OnNewEntryRegistered(NetRegistryInfo info)
            {
                var entry = info.Entry;
                PropertiesByIndex.Add(_counter++, entry);
                if (runtimeSettings.CurrentPeerType != PeerType.Server) return;

                entry.Dirty.AdviseTrue(lifetime, () =>
                {
                    _currentRegistration = info;
                    entry.Accept(this);
                    entry.Dirty.Value = false;
                });
            }
        }

        public void VisitProperty<T>(INetProperty<T> property)
        {
            SendAllClientsPropertyUpdate(property);
        }

        public void VisitSignal<T>(INetSignal<T> entry)
        {
            SendAllClientsSignalUpdate(entry);
        }

        private void SendAllClientsPropertyUpdate<T>(INetProperty<T> property)
        {
            var messageSender = (IMessageSender<T>)_messageSenders.ByType[typeof(T)];
            messageSender.SendToAllClients(
                new NetworkMessageHeader
                {
                    NetworkObjectId = _networkObject.NetworkObjectId,
                    VarId = PropertiesByIndex.Inverse[property]
                },
                property.Value,
                _currentRegistration.DeliveryType);
        }

        private void SendAllClientsSignalUpdate<T>(INetSignal<T> entry)
        {
            var messageSender = (IMessageSender<T>)_messageSenders.ByType[typeof(T)];
            messageSender.SendToAllClients(
                new NetworkMessageHeader
                {
                    NetworkObjectId = _networkObject.NetworkObjectId,
                    VarId = PropertiesByIndex.Inverse[entry]
                },
                entry.PendingPayload,
                _currentRegistration.DeliveryType);
        }
    }
}
