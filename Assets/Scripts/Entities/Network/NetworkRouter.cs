using System;
using System.Runtime.CompilerServices;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;
using Unity.Collections;
using Unity.Netcode;

namespace Bw.Entities.Network
{
    public interface IClientNetworkRouter
    {
        public void SendToServer<T>(NetworkMessage<T> message, NetworkDelivery deliveryType)
            where T : struct, INetworkSerializable;
    }
    public interface INetworkRouter
    {
        public void SendToClient<T>(NetworkMessage<T> message, NetworkDelivery deliveryType, IClient client)
            where T : struct, INetworkSerializable;
        public void Broadcast<T>(NetworkMessage<T> message, NetworkDelivery deliveryType)
            where T : struct, INetworkSerializable;
    }

    public class NetworkRouter : INetworkRouter, IClientNetworkRouter, INetPropertyVisitor //TODO: decompose
    {
        private const int ReserveSize = 16;
        private readonly NetworkManager _networkManager;

        private readonly IMessageReceivers _messageReceivers;

        private FastBufferReader _currentReader;

        public NetworkRouter(
            Lifetime lifetime,
            NetworkManager networkManager,
            IMessageReceivers receivers)
        {
            _networkManager = networkManager;
            _messageReceivers = receivers;

            networkManager.CustomMessagingManager.OnUnnamedMessage += HandleUnnamedMessage;
            lifetime.OnTermination(() =>
                networkManager.CustomMessagingManager.OnUnnamedMessage -= HandleUnnamedMessage);
        }
        
        public void SendToServer<T>(NetworkMessage<T> message, NetworkDelivery deliveryType) where T : struct, INetworkSerializable
        {
            var dataSize = Unsafe.SizeOf<T>() + ReserveSize;
            using var messageBuffer = new FastBufferWriter(dataSize, Allocator.Temp, dataSize);

            messageBuffer.WriteNetworkSerializable(message);
            _networkManager.CustomMessagingManager.SendUnnamedMessage(
                NetworkManager.ServerClientId,
                messageBuffer,
                deliveryType);
        }

        public void SendToClient<T>(NetworkMessage<T> message, NetworkDelivery deliveryType, IClient client) where T : struct, INetworkSerializable
        {
            var dataSize = Unsafe.SizeOf<T>() + ReserveSize;
            using var messageBuffer = new FastBufferWriter(dataSize, Allocator.Temp, dataSize);

            messageBuffer.WriteNetworkSerializable(message);
            _networkManager.CustomMessagingManager.SendUnnamedMessage(
                client.Id,
                messageBuffer,
                deliveryType);
        }

        public void Broadcast<T>(NetworkMessage<T> message, NetworkDelivery deliveryType)
            where T : struct, INetworkSerializable
        {
            var dataSize = Unsafe.SizeOf<T>() + ReserveSize;
            using var messageBuffer = new FastBufferWriter(dataSize, Allocator.Temp, dataSize);

            messageBuffer.WriteNetworkSerializable(message);
            _networkManager.CustomMessagingManager.SendUnnamedMessageToAll(
                messageBuffer, 
                deliveryType);
        }

        public void Visit<T>(INetProperty<T> property)
        {
            if (!_messageReceivers.ByType.TryGetValue(typeof(T), out var receiver))
                return;

            ((IMessageReceiver<T>)receiver).Receive(ref _currentReader, property);
        }
        
        private void HandleUnnamedMessage(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out NetworkMessageHeader header);

            if (!_networkManager.SpawnManager.SpawnedObjects.TryGetValue(header.NetworkObjectId, out var netObj))
                throw new Exception($"No network object with id '{header.NetworkObjectId}' was found.");

            if (!netObj.TryGetComponent<INetworkLifetimedObject>(out var targetObject))
                throw new Exception($"No network object with id '{header.NetworkObjectId}' was found.");

            var targetProperty = targetObject.NetVariablesTable.PropertiesByIndex[header.VarId];

            _currentReader = reader; 
            targetProperty.Accept(this); 
        }
    }
}