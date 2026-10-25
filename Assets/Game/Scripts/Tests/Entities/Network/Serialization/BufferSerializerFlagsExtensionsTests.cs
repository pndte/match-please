using System;
using Bw.Entities.Network.Serialization;
using Bw.Tests.Entities.Network.Doubles;
using NUnit.Framework;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Serialization
{
    public sealed class BufferSerializerFlagsExtensionsTests
    {
        [Test]
        public void TwoFlagsTakeOneByteInArgumentOrder() =>
            CheckEveryCombination(2, bits => new Flags2 { Bits = bits });

        [Test]
        public void ThreeFlagsTakeOneByteInArgumentOrder() =>
            CheckEveryCombination(3, bits => new Flags3 { Bits = bits });

        [Test]
        public void FourFlagsTakeOneByteInArgumentOrder() =>
            CheckEveryCombination(4, bits => new Flags4 { Bits = bits });

        [Test]
        public void FiveFlagsTakeOneByteInArgumentOrder() =>
            CheckEveryCombination(5, bits => new Flags5 { Bits = bits });

        [Test]
        public void SixFlagsTakeOneByteInArgumentOrder() =>
            CheckEveryCombination(6, bits => new Flags6 { Bits = bits });

        [Test]
        public void SevenFlagsTakeOneByteInArgumentOrder() =>
            CheckEveryCombination(7, bits => new Flags7 { Bits = bits });

        [Test]
        public void EightFlagsTakeOneByteInArgumentOrder() =>
            CheckEveryCombination(8, bits => new Flags8 { Bits = bits });

        private static void CheckEveryCombination<T>(int count, Func<bool[], T> of) where T : struct, INetworkSerializable, IFlags
        {
            for (var mask = 0; mask < 1 << count; mask++)
            {
                var bits = BitsOf(mask, count);

                var bytes = Wire.BytesOf(of(bits));

                Assert.That(bytes, Is.EqualTo(new[] { (byte)mask }), $"mask {mask.ToString()}");
                Assert.That(Wire.Read<T>(bytes).Bits, Is.EqualTo(bits), $"mask {mask.ToString()}");
            }
        }

        private static bool[] BitsOf(int mask, int count)
        {
            var bits = new bool[count];
            for (var index = 0; index < count; index++)
                bits[index] = (mask & (1 << index)) != 0;

            return bits;
        }

        private interface IFlags
        {
            bool[] Bits { get; }
        }

        private struct Flags2 : INetworkSerializable, IFlags
        {
            public bool[] Bits
            {
                get => new[] { _a, _b };
                set => (_a, _b) = (value[0], value[1]);
            }

            private bool _a;
            private bool _b;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializeFlags(ref _a, ref _b);
        }

        private struct Flags3 : INetworkSerializable, IFlags
        {
            public bool[] Bits
            {
                get => new[] { _a, _b, _c };
                set => (_a, _b, _c) = (value[0], value[1], value[2]);
            }

            private bool _a;
            private bool _b;
            private bool _c;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializeFlags(ref _a, ref _b, ref _c);
        }

        private struct Flags4 : INetworkSerializable, IFlags
        {
            public bool[] Bits
            {
                get => new[] { _a, _b, _c, _d };
                set => (_a, _b, _c, _d) = (value[0], value[1], value[2], value[3]);
            }

            private bool _a;
            private bool _b;
            private bool _c;
            private bool _d;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializeFlags(ref _a, ref _b, ref _c, ref _d);
        }

        private struct Flags5 : INetworkSerializable, IFlags
        {
            public bool[] Bits
            {
                get => new[] { _a, _b, _c, _d, _e };
                set => (_a, _b, _c, _d, _e) = (value[0], value[1], value[2], value[3], value[4]);
            }

            private bool _a;
            private bool _b;
            private bool _c;
            private bool _d;
            private bool _e;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializeFlags(ref _a, ref _b, ref _c, ref _d, ref _e);
        }

        private struct Flags6 : INetworkSerializable, IFlags
        {
            public bool[] Bits
            {
                get => new[] { _a, _b, _c, _d, _e, _f };
                set => (_a, _b, _c, _d, _e, _f) = (value[0], value[1], value[2], value[3], value[4], value[5]);
            }

            private bool _a;
            private bool _b;
            private bool _c;
            private bool _d;
            private bool _e;
            private bool _f;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializeFlags(ref _a, ref _b, ref _c, ref _d, ref _e, ref _f);
        }

        private struct Flags7 : INetworkSerializable, IFlags
        {
            public bool[] Bits
            {
                get => new[] { _a, _b, _c, _d, _e, _f, _g };
                set => (_a, _b, _c, _d, _e, _f, _g) = (value[0], value[1], value[2], value[3], value[4], value[5], value[6]);
            }

            private bool _a;
            private bool _b;
            private bool _c;
            private bool _d;
            private bool _e;
            private bool _f;
            private bool _g;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializeFlags(ref _a, ref _b, ref _c, ref _d, ref _e, ref _f, ref _g);
        }

        private struct Flags8 : INetworkSerializable, IFlags
        {
            public bool[] Bits
            {
                get => new[] { _a, _b, _c, _d, _e, _f, _g, _h };
                set => (_a, _b, _c, _d, _e, _f, _g, _h) = (value[0], value[1], value[2], value[3], value[4], value[5], value[6], value[7]);
            }

            private bool _a;
            private bool _b;
            private bool _c;
            private bool _d;
            private bool _e;
            private bool _f;
            private bool _g;
            private bool _h;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializeFlags(ref _a, ref _b, ref _c, ref _d, ref _e, ref _f, ref _g, ref _h);
        }
    }
}
