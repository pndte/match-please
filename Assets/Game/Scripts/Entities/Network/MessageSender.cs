using Bw.Entities.Network.Routing;
using Unity.Netcode;

namespace Bw.Entities.Network
{
    public sealed class MessageSender<TValue, TCodec> : IMessageSender<TValue>
        where TCodec : struct, ICodec<TValue>
    {
        private readonly IClientNetworkRouter _clientRouter;
        private readonly IServerNetworkRouter _serverRouter;

        public MessageSender(IClientNetworkRouter clientRouter, IServerNetworkRouter serverRouter)
        {
            _clientRouter = clientRouter;
            _serverRouter = serverRouter;
        }

        public void SendToAllClients(NetworkMessageHeader metadata, TValue payload, NetworkDelivery delivery)
        {
            var message = new NetworkMessage<TCodec>(metadata, Codec(payload));
            _serverRouter.Broadcast(message, delivery);
        }

        public void SendToClient(NetworkMessageHeader metadata, TValue payload, NetworkDelivery delivery, IClient client)
        {
            var message = new NetworkMessage<TCodec>(metadata, Codec(payload));
            _serverRouter.SendToClient(message, delivery, client);
        }

        public void SendToServer(NetworkMessageHeader metadata, TValue payload, NetworkDelivery delivery)
        {
            var message = new NetworkMessage<TCodec>(metadata, Codec(payload));
            _clientRouter.SendToServer(message, delivery);
        }

        private static TCodec Codec(TValue payload) =>
            new TCodec { Value = payload };
    }
}
