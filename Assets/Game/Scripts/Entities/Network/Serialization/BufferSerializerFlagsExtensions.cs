using Unity.Netcode;

namespace Bw.Entities.Network.Serialization
{
    public static class BufferSerializerFlagsExtensions
    {
        public static void SerializeFlags<T>(this BufferSerializer<T> serializer, ref bool first, ref bool second) where T : IReaderWriter
        {
            var none = false;
            serializer.SerializeFlags(ref first, ref second, ref none, ref none, ref none, ref none, ref none, ref none);
        }

        public static void SerializeFlags<T>(
            this BufferSerializer<T> serializer,
            ref bool first,
            ref bool second,
            ref bool third) where T : IReaderWriter
        {
            var none = false;
            serializer.SerializeFlags(ref first, ref second, ref third, ref none, ref none, ref none, ref none, ref none);
        }

        public static void SerializeFlags<T>(
            this BufferSerializer<T> serializer,
            ref bool first,
            ref bool second,
            ref bool third,
            ref bool fourth) where T : IReaderWriter
        {
            var none = false;
            serializer.SerializeFlags(ref first, ref second, ref third, ref fourth, ref none, ref none, ref none, ref none);
        }

        public static void SerializeFlags<T>(
            this BufferSerializer<T> serializer,
            ref bool first,
            ref bool second,
            ref bool third,
            ref bool fourth,
            ref bool fifth) where T : IReaderWriter
        {
            var none = false;
            serializer.SerializeFlags(ref first, ref second, ref third, ref fourth, ref fifth, ref none, ref none, ref none);
        }

        public static void SerializeFlags<T>(
            this BufferSerializer<T> serializer,
            ref bool first,
            ref bool second,
            ref bool third,
            ref bool fourth,
            ref bool fifth,
            ref bool sixth) where T : IReaderWriter
        {
            var none = false;
            serializer.SerializeFlags(ref first, ref second, ref third, ref fourth, ref fifth, ref sixth, ref none, ref none);
        }

        public static void SerializeFlags<T>(
            this BufferSerializer<T> serializer,
            ref bool first,
            ref bool second,
            ref bool third,
            ref bool fourth,
            ref bool fifth,
            ref bool sixth,
            ref bool seventh) where T : IReaderWriter
        {
            var none = false;
            serializer.SerializeFlags(ref first, ref second, ref third, ref fourth, ref fifth, ref sixth, ref seventh, ref none);
        }

        public static void SerializeFlags<T>(
            this BufferSerializer<T> serializer,
            ref bool first,
            ref bool second,
            ref bool third,
            ref bool fourth,
            ref bool fifth,
            ref bool sixth,
            ref bool seventh,
            ref bool eighth) where T : IReaderWriter
        {
            var bits = serializer.IsWriter
                ? (byte)(Bit(first, 0) | Bit(second, 1) | Bit(third, 2) | Bit(fourth, 3) | Bit(fifth, 4) | Bit(sixth, 5) | Bit(seventh, 6) | Bit(eighth, 7))
                : (byte)0;

            serializer.SerializeValue(ref bits);
            first = IsSet(bits, 0);
            second = IsSet(bits, 1);
            third = IsSet(bits, 2);
            fourth = IsSet(bits, 3);
            fifth = IsSet(bits, 4);
            sixth = IsSet(bits, 5);
            seventh = IsSet(bits, 6);
            eighth = IsSet(bits, 7);
        }

        private static int Bit(bool value, int index) =>
            value ? 1 << index : 0;

        private static bool IsSet(byte bits, int index) =>
            (bits & (1 << index)) != 0;
    }
}
