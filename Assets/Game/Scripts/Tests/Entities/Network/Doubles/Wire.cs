using Unity.Collections;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Doubles
{
    public static class Wire
    {
        private const int InitialSize = 64;
        private const int MaxSize = 4096;

        public static byte[] BytesOf<T>(T value) where T : INetworkSerializable
        {
            using var writer = new FastBufferWriter(InitialSize, Allocator.Temp, MaxSize);
            writer.WriteNetworkSerializable(value);
            return writer.ToArray();
        }

        public static T Read<T>(byte[] bytes) where T : INetworkSerializable, new()
        {
            using var reader = new FastBufferReader(bytes, Allocator.Temp);
            reader.ReadNetworkSerializable(out T value);
            return value;
        }

        public static T RoundTrip<T>(T value, out int length) where T : INetworkSerializable, new()
        {
            var bytes = BytesOf(value);
            length = bytes.Length;
            return Read<T>(bytes);
        }

        public static T RoundTrip<T>(T value) where T : INetworkSerializable, new() =>
            RoundTrip(value, out _);
    }
}
