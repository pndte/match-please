using Bw.Entities.Network;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Doubles
{
    public sealed class CountingSenders : IClientSendersCollection, IServerSendersCollection
    {
        public int Sent { get; private set; }

        IClientMessageSender<T> IClientSendersCollection.Get<T>() =>
            new Counted<T>(this);

        IServerMessageSender<T> IServerSendersCollection.Get<T>() =>
            new Counted<T>(this);

        private sealed class Counted<T> : IMessageSender<T>
        {
            private readonly CountingSenders _owner;

            public Counted(CountingSenders owner)
            {
                _owner = owner;
            }

            public void SendToServer(NetworkMessageHeader metadata, T payload, NetworkDelivery delivery) =>
                _owner.Sent++;

            public void Broadcast(NetworkMessageHeader metadata, T payload, NetworkDelivery delivery) =>
                _owner.Sent++;

            public void SendToClient(
                NetworkMessageHeader metadata,
                T payload,
                NetworkDelivery delivery,
                IClient recipient) =>
                _owner.Sent++;
        }
    }
}
