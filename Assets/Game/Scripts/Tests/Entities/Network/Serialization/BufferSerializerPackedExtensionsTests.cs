using Bw.Entities.Network.Serialization;
using Bw.Tests.Entities.Network.Doubles;
using NUnit.Framework;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Serialization
{
    public sealed class BufferSerializerPackedExtensionsTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void BoolTakesOneByte(bool value)
        {
            var decoded = Wire.RoundTrip(new PackedBool { Value = value }, out var length);

            Assert.That(decoded.Value, Is.EqualTo(value));
            Assert.That(length, Is.EqualTo(1));
        }

        [TestCase((byte)0)]
        [TestCase((byte)127)]
        [TestCase(byte.MaxValue)]
        public void ByteTakesOneByte(byte value)
        {
            var decoded = Wire.RoundTrip(new PackedByte { Value = value }, out var length);

            Assert.That(decoded.Value, Is.EqualTo(value));
            Assert.That(length, Is.EqualTo(1));
        }

        [TestCase((ushort)0, 1)]
        [TestCase((ushort)63, 1)]
        [TestCase((ushort)64, 2)]
        [TestCase((ushort)16383, 2)]
        [TestCase((ushort)16384, 3)]
        [TestCase(ushort.MaxValue, 3)]
        public void UshortIsPackedBySize(ushort value, int bytes)
        {
            var decoded = Wire.RoundTrip(new PackedUshort { Value = value }, out var length);

            Assert.That(decoded.Value, Is.EqualTo(value));
            Assert.That(length, Is.EqualTo(bytes));
        }

        [TestCase(0U, 1)]
        [TestCase(31U, 1)]
        [TestCase(32U, 2)]
        [TestCase(8191U, 2)]
        [TestCase(8192U, 3)]
        [TestCase(2097151U, 3)]
        [TestCase(2097152U, 4)]
        [TestCase(536870911U, 4)]
        [TestCase(536870912U, 5)]
        [TestCase(uint.MaxValue, 5)]
        public void UintIsPackedBySize(uint value, int bytes)
        {
            var decoded = Wire.RoundTrip(new PackedUint { Value = value }, out var length);

            Assert.That(decoded.Value, Is.EqualTo(value));
            Assert.That(length, Is.EqualTo(bytes));
        }

        [TestCase(0, 1)]
        [TestCase(15, 1)]
        [TestCase(-16, 1)]
        [TestCase(16, 2)]
        [TestCase(-17, 2)]
        [TestCase(4095, 2)]
        [TestCase(-4096, 2)]
        [TestCase(4096, 3)]
        [TestCase(-268435456, 4)]
        [TestCase(268435456, 5)]
        [TestCase(int.MaxValue, 5)]
        [TestCase(int.MinValue, 5)]
        public void IntIsPackedByMagnitudeWithTheSignInTheLowestBit(int value, int bytes)
        {
            var decoded = Wire.RoundTrip(new PackedInt { Value = value }, out var length);

            Assert.That(decoded.Value, Is.EqualTo(value));
            Assert.That(length, Is.EqualTo(bytes));
        }

        [TestCase(0UL, 1)]
        [TestCase(15UL, 1)]
        [TestCase(16UL, 2)]
        [TestCase(4095UL, 2)]
        [TestCase(4096UL, 3)]
        [TestCase((1UL << 20) - 1, 3)]
        [TestCase(1UL << 20, 4)]
        [TestCase((1UL << 28) - 1, 4)]
        [TestCase(1UL << 28, 5)]
        [TestCase((1UL << 36) - 1, 5)]
        [TestCase(1UL << 36, 6)]
        [TestCase((1UL << 44) - 1, 6)]
        [TestCase(1UL << 44, 7)]
        [TestCase((1UL << 52) - 1, 7)]
        [TestCase(1UL << 52, 8)]
        [TestCase((1UL << 60) - 1, 8)]
        [TestCase(1UL << 60, 9)]
        [TestCase(ulong.MaxValue, 9)]
        public void UlongIsPackedBySize(ulong value, int bytes)
        {
            var decoded = Wire.RoundTrip(new PackedUlong { Value = value }, out var length);

            Assert.That(decoded.Value, Is.EqualTo(value));
            Assert.That(length, Is.EqualTo(bytes));
        }

        [TestCase(0L, 1)]
        [TestCase(7L, 1)]
        [TestCase(-8L, 1)]
        [TestCase(8L, 2)]
        [TestCase(-9L, 2)]
        [TestCase(2047L, 2)]
        [TestCase(-2048L, 2)]
        [TestCase(2048L, 3)]
        [TestCase((1L << 59) - 1, 8)]
        [TestCase(-(1L << 59), 8)]
        [TestCase(1L << 59, 9)]
        [TestCase(long.MaxValue, 9)]
        [TestCase(long.MinValue, 9)]
        public void LongIsPackedByMagnitudeWithTheSignInTheLowestBit(long value, int bytes)
        {
            var decoded = Wire.RoundTrip(new PackedLong { Value = value }, out var length);

            Assert.That(decoded.Value, Is.EqualTo(value));
            Assert.That(length, Is.EqualTo(bytes));
        }

        private struct PackedBool : INetworkSerializable
        {
            public bool Value;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializePacked(ref Value);
        }

        private struct PackedByte : INetworkSerializable
        {
            public byte Value;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializePacked(ref Value);
        }

        private struct PackedUshort : INetworkSerializable
        {
            public ushort Value;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializePacked(ref Value);
        }

        private struct PackedUint : INetworkSerializable
        {
            public uint Value;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializePacked(ref Value);
        }

        private struct PackedInt : INetworkSerializable
        {
            public int Value;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializePacked(ref Value);
        }

        private struct PackedUlong : INetworkSerializable
        {
            public ulong Value;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializePacked(ref Value);
        }

        private struct PackedLong : INetworkSerializable
        {
            public long Value;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializePacked(ref Value);
        }
    }
}
