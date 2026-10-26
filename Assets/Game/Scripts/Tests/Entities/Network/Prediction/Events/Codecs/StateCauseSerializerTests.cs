using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Events.Codecs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Prediction.Events.Codecs
{
    public sealed class StateCauseSerializerTests
    {
        [Test]
        public void NoneTakesOneByte()
        {
            using var writer = new FastBufferWriter(64, Allocator.Temp);

            writer.WriteNetworkSerializable(new CauseCodec { Cause = StateCause.None });

            Assert.That(writer.Length, Is.EqualTo(1));
        }

        [Test]
        public void NoneComesBackAsNone()
        {
            Assert.That(StateCauseTests.Described(RoundTrip(StateCause.None, out _)), Is.EqualTo("none"));
        }

        [TestCase(0UL, 0, 3)]
        [TestCase(3UL, 7, 3)]
        [TestCase(5UL, 100, 4)]
        [TestCase(1000UL, 54321, 6)]
        [TestCase(ulong.MaxValue, int.MaxValue, 15)]
        [TestCase(1UL, -3, 3)]
        public void ActionComesBackWithItsInitiatorAndTick(ulong initiator, int tick, int bytes)
        {
            var decoded = RoundTrip(StateCause.Of(new ActionId(initiator, tick)), out var length);

            Assert.That(StateCauseTests.Described(decoded), Is.EqualTo($"{initiator.ToString()}@{tick.ToString()}"));
            Assert.That(length, Is.EqualTo(bytes));
        }

        [Test]
        public void ReaderReadsExactlyWhatTheWriterWrote()
        {
            using var writer = new FastBufferWriter(64, Allocator.Temp);
            writer.WriteNetworkSerializable(new CauseCodec { Cause = StateCause.Of(new ActionId(12UL, 345)) });
            writer.WriteNetworkSerializable(new CauseCodec { Cause = StateCause.None });
            using var reader = new FastBufferReader(writer, Allocator.Temp);

            reader.ReadNetworkSerializable(out CauseCodec first);
            reader.ReadNetworkSerializable(out CauseCodec second);

            Assert.That(StateCauseTests.Described(first.Cause), Is.EqualTo("12@345"));
            Assert.That(StateCauseTests.Described(second.Cause), Is.EqualTo("none"));
            Assert.That(reader.Position, Is.EqualTo(writer.Length));
        }

        private static StateCause RoundTrip(StateCause cause, out int length)
        {
            using var writer = new FastBufferWriter(64, Allocator.Temp);
            writer.WriteNetworkSerializable(new CauseCodec { Cause = cause });
            length = writer.Length;
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out CauseCodec decoded);
            return decoded.Cause;
        }

        private struct CauseCodec : INetworkSerializable
        {
            public StateCause Cause;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializeCause(ref Cause);
        }
    }
}
