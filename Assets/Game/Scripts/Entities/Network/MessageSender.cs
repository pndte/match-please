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

        public void SendToServer(NetworkMessageHeader metadata, TValue payload, NetworkDelivery delivery) =>
            _clientRouter.SendToServer(ToMessage(metadata, payload), delivery);

        public void Broadcast(NetworkMessageHeader metadata, TValue payload, NetworkDelivery delivery) =>
            _serverRouter.Broadcast(ToMessage(metadata, payload), delivery);

        public void SendToClient(NetworkMessageHeader metadata, TValue payload, NetworkDelivery delivery, IClient recipient) =>
            _serverRouter.SendToClient(ToMessage(metadata, payload), delivery, recipient);

        private static NetworkMessage<TCodec> ToMessage(NetworkMessageHeader metadata, TValue payload) =>
            new(metadata, new TCodec { Value = payload });
    }
}
