using Unity.Netcode;

namespace Bw.Entities.Network
{
    public interface IServerMessageSender<T>
    {
        void Broadcast(NetworkMessageHeader metadata, T payload, NetworkDelivery delivery);
        void SendToClient(NetworkMessageHeader metadata, T payload, NetworkDelivery delivery, IClient recipient);
    }
}
