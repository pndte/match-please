using Unity.Netcode;

namespace Bw.Entities.Network
{
    public interface IClientMessageSender<in T>
    {
        void SendToServer(NetworkMessageHeader metadata, T payload, NetworkDelivery delivery);
    }
}
