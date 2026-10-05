using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    internal struct NetSchemaHash : INetworkSerializable
    {
        public uint Value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
            serializer.SerializeValue(ref Value);
    }
}
