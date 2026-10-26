using Unity.Collections;
using Unity.Netcode;

namespace Bw.Entities.Network.Serialization
{
    public static class CodecRoundTrip //TODO: костыль
    {
        private const int InitialSize = 64;

        public static TValue Received<TValue, TCodec>(TValue sent) where TCodec : struct, ICodec<TValue>
        {
            using var writer = new FastBufferWriter(InitialSize, Allocator.Temp, ushort.MaxValue);
            writer.WriteNetworkSerializable(new TCodec { Value = sent });
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out TCodec received);
            return received.Value;
        }
    }
}
