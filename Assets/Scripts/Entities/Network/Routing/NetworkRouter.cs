using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Netcode;

namespace Bw.Entities.Network.Routing
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

    public class NetworkRouter : INetworkRouter, IClientNetworkRouter
    {
        private const int ReserveSize = 16;
        private readonly INetworkHolder _networkHolder;
        
        public NetworkRouter(INetworkHolder networkHolder)
        {
            _networkHolder = networkHolder;
        }
        
        public void SendToServer<T>(NetworkMessage<T> message, NetworkDelivery deliveryType) where T : struct, INetworkSerializable
        {
            var dataSize = Unsafe.SizeOf<T>() + ReserveSize;
            using var messageBuffer = new FastBufferWriter(dataSize, Allocator.Temp, dataSize);

            messageBuffer.WriteNetworkSerializable(message);
            _networkHolder.CustomMessagingManager().SendUnnamedMessage(
                NetworkManager.ServerClientId,
                messageBuffer,
                deliveryType);
        }

        public void SendToClient<T>(NetworkMessage<T> message, NetworkDelivery deliveryType, IClient client) where T : struct, INetworkSerializable
        {
            var dataSize = Unsafe.SizeOf<T>() + ReserveSize;
            using var messageBuffer = new FastBufferWriter(dataSize, Allocator.Temp, dataSize);

            messageBuffer.WriteNetworkSerializable(message);
            _networkHolder.CustomMessagingManager().SendUnnamedMessage(
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
            _networkHolder.CustomMessagingManager().SendUnnamedMessageToAll(
                messageBuffer, 
                deliveryType);
        }
    }
}