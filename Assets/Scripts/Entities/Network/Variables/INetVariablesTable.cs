using Bw.Entities.Extensions;
using Bw.Entities.Infrastructure;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Entities.Network.Variables
{
    public interface INetVariablesTable
    {
        public IViewableBiMap<ushort, INetProperty> PropertiesByIndex { get; }
    }

    public class NetVariablesTable : INetVariablesTable, INetPropertyVisitor
    {
        public IViewableBiMap<ushort, INetProperty> PropertiesByIndex { get; }

        private readonly NetworkObject _networkObject; //TODO: придумать что-то, нужно только для id
        private readonly IMessageSenders _messageSenders;

        private ushort _counter = 1;
        private NetPropertyInfo _currentPropertyInfo;
        
        public NetVariablesTable(
            Lifetime lifetime, 
            NetworkObject networkObject,
            IMessageSenders senders,
            INetPropertyFactory factory,
            IRuntimeSettings runtimeSettings)
        {
            PropertiesByIndex = new ViewableBiMap<ushort, INetProperty>(lifetime);
            _networkObject = networkObject;
            _messageSenders = senders;
            factory.PropertyRegistered.Advise(lifetime, OnNewPropertyRegistered);
            return;
            
            void OnNewPropertyRegistered(NetPropertyInfo info)
            {
                var netProperty = info.Property;
                PropertiesByIndex.Add(_counter++, netProperty);
                if (runtimeSettings.CurrentPeerType != PeerType.Server) return; //TODO: полноценная система разрешений для переменных
                
                netProperty.Dirty.AdviseTrue(lifetime, () =>
                {
                    _currentPropertyInfo = info;
                    netProperty.Accept(this);
                    netProperty.Dirty.Value = false; //TODO: очень важно ждать наступление след сетевого тика. Для этого надо делать свои степраннеры
                });
            }
        }
        public void Visit<T>(INetProperty<T> property)
        {
            SendAllClientsVariableUpdate(property);
        }

        private void SendAllClientsVariableUpdate<T>(INetProperty<T> property)
        {
            var messageSender = (IMessageSender<T>)_messageSenders.ByType[typeof(T)];
            messageSender.SendToAllClients(
                new NetworkMessageHeader()
                {
                    NetworkObjectId = _networkObject.NetworkObjectId,
                    VarId = PropertiesByIndex.Inverse[property]
                },
                property.Value,
                _currentPropertyInfo.DeliveryType);
        }
    }
}