using Unity.Netcode;

namespace Bw.Entities.Network
{
    public interface IMessageSender
    { }
    
    public interface IMessageSender<in T> : IMessageSender
    {
        void SendToAllClients(NetworkMessageHeader metadata, T payload, NetworkDelivery delivery);
    }
    
    public class MessageSender<TValue, TCodec> : IMessageSender<TValue>
        where TCodec : struct, ICodec<TValue>
    {
        private readonly INetworkRouter _router;

        public MessageSender(INetworkRouter router)
        {
            _router = router;
        }
        
        public void SendToAllClients(NetworkMessageHeader metadata, TValue payload, NetworkDelivery delivery)
        {
            var codec = new TCodec
            {
                Value = payload
            };

            var message = new NetworkMessage<TCodec>(metadata, codec);
            _router.Broadcast(message, delivery);
        }
    }
}