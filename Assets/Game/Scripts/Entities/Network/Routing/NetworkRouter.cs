using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Netcode;

namespace Bw.Entities.Network.Routing
{
    /// <summary>Client-side routing: messages always go to the server.</summary>
    public interface IClientNetworkRouter
    {
        void SendToServer<T>(NetworkMessage<T> message, NetworkDelivery deliveryType)
            where T : struct, INetworkSerializable;
    }

    /// <summary>Server-side routing: broadcast or target a specific connected client.</summary>
    public interface IServerNetworkRouter
    {
        void SendToClient<T>(NetworkMessage<T> message, NetworkDelivery deliveryType, IClient client)
            where T : struct, INetworkSerializable;

        void Broadcast<T>(NetworkMessage<T> message, NetworkDelivery deliveryType)
            where T : struct, INetworkSerializable;
    }

    public sealed class NetworkRouter : IClientNetworkRouter, IServerNetworkRouter
    {
        private const int ReserveSize = 16;
        private const int MaxMessageSize = ushort.MaxValue;

        private bool ShuttingDown => _networkHolder.NetworkManager.Value.ShutdownInProgress;

        private readonly INetworkHolder _networkHolder;
        private readonly IClientCollection _clients;

        public NetworkRouter(INetworkHolder networkHolder, IClientCollection clients)
        {
            _networkHolder = networkHolder;
            _clients = clients;
        }

        public void SendToServer<T>(NetworkMessage<T> message, NetworkDelivery deliveryType)
            where T : struct, INetworkSerializable
        {
            if (ShuttingDown)
                return;

            var dataSize = Unsafe.SizeOf<T>() + ReserveSize;
            using var messageBuffer = new FastBufferWriter(dataSize, Allocator.Temp, MaxMessageSize);

            messageBuffer.WriteNetworkSerializable(message);
            _networkHolder.CustomMessagingManager().SendUnnamedMessage(
                NetworkManager.ServerClientId,
                messageBuffer,
                deliveryType);
        }

        public void SendToClient<T>(NetworkMessage<T> message, NetworkDelivery deliveryType, IClient client)
            where T : struct, INetworkSerializable
        {
            if (ShuttingDown)
                return;

            var dataSize = Unsafe.SizeOf<T>() + ReserveSize;
            using var messageBuffer = new FastBufferWriter(dataSize, Allocator.Temp, MaxMessageSize);

            messageBuffer.WriteNetworkSerializable(message);
            _networkHolder.CustomMessagingManager().SendUnnamedMessage(
                _clients.IdOf(client),
                messageBuffer,
                deliveryType);
        }

        public void Broadcast<T>(NetworkMessage<T> message, NetworkDelivery deliveryType)
            where T : struct, INetworkSerializable
        {
            var recipients = _networkHolder.NetworkManager.Value.ConnectedClientsIds;
            if (ShuttingDown || recipients.Count == 0)
                return;

            var dataSize = Unsafe.SizeOf<T>() + ReserveSize;
            using var messageBuffer = new FastBufferWriter(dataSize, Allocator.Temp, MaxMessageSize);

            messageBuffer.WriteNetworkSerializable(message);
            _networkHolder.CustomMessagingManager().SendUnnamedMessage(
                recipients,
                messageBuffer,
                deliveryType);
        }
    }
}
