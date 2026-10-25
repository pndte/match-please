using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.Routing;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Doubles
{
    public sealed class RecordingRouter : IClientNetworkRouter, IServerNetworkRouter
    {
        public IReadOnlyList<Routed> ToServer => _toServer;
        public IReadOnlyList<Routed> Broadcasts => _broadcasts;
        public IReadOnlyList<(IClient Recipient, Routed Message)> ToClients => _toClients;

        private readonly List<Routed> _toServer = new();
        private readonly List<Routed> _broadcasts = new();
        private readonly List<(IClient Recipient, Routed Message)> _toClients = new();

        public void SendToServer<T>(NetworkMessage<T> message, NetworkDelivery deliveryType)
            where T : struct, INetworkSerializable =>
            _toServer.Add(Routed.Of(message, deliveryType));

        public void SendToClient<T>(NetworkMessage<T> message, NetworkDelivery deliveryType, IClient client)
            where T : struct, INetworkSerializable =>
            _toClients.Add((client, Routed.Of(message, deliveryType)));

        public void Broadcast<T>(NetworkMessage<T> message, NetworkDelivery deliveryType)
            where T : struct, INetworkSerializable =>
            _broadcasts.Add(Routed.Of(message, deliveryType));

        public sealed class Routed
        {
            public object Message { get; }
            public byte[] Bytes { get; }
            public NetworkDelivery Delivery { get; }

            private Routed(object message, byte[] bytes, NetworkDelivery delivery)
            {
                Message = message;
                Bytes = bytes;
                Delivery = delivery;
            }

            public static Routed Of<T>(NetworkMessage<T> message, NetworkDelivery delivery)
                where T : struct, INetworkSerializable =>
                new(message, Wire.BytesOf(message), delivery);
        }
    }
}
