using Bw.Entities.Network.Routing;
using Unity.Netcode;

namespace Bw.Entities.Network
{
    public sealed class TargetedMessageSender<TValue, TCodec> : MessageSender<TValue, TCodec>
        where TCodec : struct, ICodec<TValue>
    {
        private readonly CodecTargetRouting<TCodec> _routing;

        public TargetedMessageSender(
            IClientNetworkRouter clientRouter,
            IServerNetworkRouter serverRouter,
            CodecTargetRouting<TCodec> routing)
            : base(clientRouter, serverRouter)
        {
            _routing = routing;
        }

        public override void Dispatch(NetworkMessageHeader metadata, TValue payload, NetworkDelivery delivery)
        {
            var codec = ToCodec(payload);
            var message = new NetworkMessage<TCodec>(metadata, codec);
            ServerRouter.SendToClient(message, delivery, _routing.GetTargetClientFromCodec(codec));
        }
    }
}
