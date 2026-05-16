using Unity.Netcode;

namespace Bw.Entities.Network
{
    public interface IServerMessageSender<T>
    {
        void SendToAllClients(NetworkMessageHeader metadata, T payload, NetworkDelivery delivery);

        void SendToClient(NetworkMessageHeader metadata, T payload, NetworkDelivery delivery, IClient client);
    }
}
