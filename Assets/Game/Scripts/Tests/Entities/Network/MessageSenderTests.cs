using Bw.Entities.Network;
using Bw.Entities.Network.Codecs;
using Bw.Tests.Entities.Network.Doubles;
using NUnit.Framework;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network
{
    public sealed class MessageSenderTests
    {
        private static readonly NetworkMessageHeader Header = new() { NetworkObjectId = 12, VarId = 4 };

        private RecordingRouter _router;
        private MessageSender<int, IntCodec> _sender;

        [SetUp]
        public void SetUp()
        {
            _router = new RecordingRouter();
            _sender = new MessageSender<int, IntCodec>(_router, _router);
        }

        [Test]
        public void SendToServerWrapsThePayloadInItsCodec()
        {
            _sender.SendToServer(Header, 300, NetworkDelivery.Unreliable);

            Assert.That(_router.ToServer, Has.Count.EqualTo(1));
            Assert.That(_router.ToServer[0].Message, Is.TypeOf<NetworkMessage<IntCodec>>());
            Assert.That(_router.ToServer[0].Delivery, Is.EqualTo(NetworkDelivery.Unreliable));
            var decoded = Wire.Read<NetworkMessage<IntCodec>>(_router.ToServer[0].Bytes);
            Assert.That(decoded.Meta.NetworkObjectId, Is.EqualTo(12UL));
            Assert.That(decoded.Meta.VarId, Is.EqualTo(4));
            Assert.That(decoded.Payload.Value, Is.EqualTo(300));
        }

        [Test]
        public void BroadcastGoesToEveryClient()
        {
            _sender.Broadcast(Header, -1, NetworkDelivery.ReliableSequenced);

            Assert.That(_router.Broadcasts, Has.Count.EqualTo(1));
            Assert.That(_router.Broadcasts[0].Delivery, Is.EqualTo(NetworkDelivery.ReliableSequenced));
            Assert.That(Wire.Read<NetworkMessage<IntCodec>>(_router.Broadcasts[0].Bytes).Payload.Value, Is.EqualTo(-1));
            Assert.That(_router.ToServer, Is.Empty);
            Assert.That(_router.ToClients, Is.Empty);
        }

        [Test]
        public void SendToClientGoesOnlyToItsRecipient()
        {
            var recipient = new Client(new ConnectionRequest("Сева", 4, System.Guid.NewGuid()));

            _sender.SendToClient(Header, 9, NetworkDelivery.Reliable, recipient);

            Assert.That(_router.ToClients, Has.Count.EqualTo(1));
            Assert.That(_router.ToClients[0].Recipient, Is.SameAs(recipient));
            Assert.That(_router.ToClients[0].Message.Delivery, Is.EqualTo(NetworkDelivery.Reliable));
            Assert.That(Wire.Read<NetworkMessage<IntCodec>>(_router.ToClients[0].Message.Bytes).Payload.Value, Is.EqualTo(9));
            Assert.That(_router.Broadcasts, Is.Empty);
        }
    }
}
