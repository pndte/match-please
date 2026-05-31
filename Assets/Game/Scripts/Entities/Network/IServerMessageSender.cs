using Unity.Netcode;

namespace Bw.Entities.Network
{
    public interface IServerMessageSender<T>
    {
        void Dispatch(NetworkMessageHeader metadata, T payload, NetworkDelivery delivery); //TODO: Надо придумать норм имя, сейчас в методе
                                                                                           //идёт отправка и всем клиентам, и одному определённому
    }
}
