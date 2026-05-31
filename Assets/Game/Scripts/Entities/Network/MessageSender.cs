using Bw.Entities.Network.Routing;
using Unity.Netcode;

namespace Bw.Entities.Network
{
    public class MessageSender<TValue, TCodec> : IMessageSender<TValue>
        where TCodec : struct, ICodec<TValue>
    {
        protected readonly IClientNetworkRouter ClientRouter;
        protected readonly IServerNetworkRouter ServerRouter;

        public MessageSender(IClientNetworkRouter clientRouter, IServerNetworkRouter serverRouter)
        {
            ClientRouter = clientRouter;
            ServerRouter = serverRouter;
        }

        public void SendToServer(NetworkMessageHeader metadata, TValue payload, NetworkDelivery delivery)
        {
            var message = new NetworkMessage<TCodec>(metadata, ToCodec(payload));
            ClientRouter.SendToServer(message, delivery);
        }

        public virtual void Dispatch(NetworkMessageHeader metadata, TValue payload, NetworkDelivery delivery)
        {
            var message = new NetworkMessage<TCodec>(metadata, ToCodec(payload));
            ServerRouter.Broadcast(message, delivery);
        }

        protected static TCodec ToCodec(TValue payload) => new() { Value = payload };
    }
}
