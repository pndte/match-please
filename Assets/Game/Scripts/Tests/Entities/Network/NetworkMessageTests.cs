using Bw.Entities.Network;
using Bw.Entities.Network.Codecs;
using Bw.Tests.Entities.Network.Doubles;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network
{
    public sealed class NetworkMessageTests
    {
        [TestCase(0UL, (ushort)0, 2)]
        [TestCase(15UL, (ushort)63, 2)]
        [TestCase(16UL, (ushort)63, 3)]
        [TestCase(15UL, (ushort)64, 3)]
        [TestCase(4095UL, (ushort)16383, 4)]
        [TestCase(4096UL, (ushort)16384, 6)]
        [TestCase(ulong.MaxValue, ushort.MaxValue, 12)]
        public void HeaderPacksTheObjectIdAndTheEntryId(ulong objectId, ushort varId, int bytes)
        {
            var header = new NetworkMessageHeader { NetworkObjectId = objectId, VarId = varId };

            var decoded = Wire.RoundTrip(header, out var length);

            Assert.That(decoded.NetworkObjectId, Is.EqualTo(objectId));
            Assert.That(decoded.VarId, Is.EqualTo(varId));
            Assert.That(length, Is.EqualTo(bytes));
        }

        [Test]
        public void MessageCarriesTheHeaderAndThenThePayload()
        {
            var message = new NetworkMessage<IntCodec>(
                new NetworkMessageHeader { NetworkObjectId = 42, VarId = 3 },
                new IntCodec { Value = -7 });

            var decoded = Wire.RoundTrip(message, out var length);

            Assert.That(decoded.Meta.NetworkObjectId, Is.EqualTo(42UL));
            Assert.That(decoded.Meta.VarId, Is.EqualTo(3));
            Assert.That(decoded.Payload.Value, Is.EqualTo(-7));
            Assert.That(length, Is.EqualTo(Wire.BytesOf(message.Meta).Length + Wire.BytesOf(message.Payload).Length));
        }

        [Test]
        public void HeaderBytesComeFirst()
        {
            var header = new NetworkMessageHeader { NetworkObjectId = 1000, VarId = 9 };
            var message = new NetworkMessage<BoolCodec>(header, new BoolCodec { Value = true });

            var bytes = Wire.BytesOf(message);

            Assert.That(Wire.Read<NetworkMessageHeader>(bytes).NetworkObjectId, Is.EqualTo(1000UL));
            Assert.That(bytes[^1], Is.EqualTo(1));
        }
    }
}
