using System.Collections.Generic;
using Bw.Entities.Network;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Doubles
{
    public sealed class RecordingSenders : IClientSendersCollection, IServerSendersCollection
    {
        public IReadOnlyList<Sent> All => _all;

        private readonly List<Sent> _all = new();

        IClientMessageSender<T> IClientSendersCollection.Get<T>() =>
            new Recorder<T>(this);

        IServerMessageSender<T> IServerSendersCollection.Get<T>() =>
            new Recorder<T>(this);

        public enum Route
        {
            ToServer,
            Broadcast,
            ToClient
        }

        public sealed class Sent
        {
            public Route Route { get; }
            public NetworkMessageHeader Header { get; }
            public object Payload { get; }
            public NetworkDelivery Delivery { get; }
            public IClient Recipient { get; }

            public Sent(
                Route route,
                NetworkMessageHeader header,
                object payload,
                NetworkDelivery delivery,
                IClient recipient)
            {
                Route = route;
                Header = header;
                Payload = payload;
                Delivery = delivery;
                Recipient = recipient;
            }
        }

        private sealed class Recorder<T> : IMessageSender<T>
        {
            private readonly RecordingSenders _owner;

            public Recorder(RecordingSenders owner)
            {
                _owner = owner;
            }

            public void SendToServer(NetworkMessageHeader metadata, T payload, NetworkDelivery delivery) =>
                _owner._all.Add(new Sent(Route.ToServer, metadata, payload, delivery, null));

            public void Broadcast(NetworkMessageHeader metadata, T payload, NetworkDelivery delivery) =>
                _owner._all.Add(new Sent(Route.Broadcast, metadata, payload, delivery, null));

            public void SendToClient(
                NetworkMessageHeader metadata,
                T payload,
                NetworkDelivery delivery,
                IClient recipient) =>
                _owner._all.Add(new Sent(Route.ToClient, metadata, payload, delivery, recipient));
        }
    }
}
