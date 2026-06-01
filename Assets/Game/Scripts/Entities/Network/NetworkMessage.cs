using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.Entities.Network
{
    public struct NetworkMessage<TPayload> : INetworkSerializable where TPayload: struct, INetworkSerializable
    {
        public NetworkMessageHeader Meta;
        public TPayload Payload;

        public NetworkMessage(NetworkMessageHeader meta, TPayload payload)
        {
            Meta = meta;
            Payload = payload;
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            Meta.NetworkSerialize(serializer);
            Payload.NetworkSerialize(serializer);
        }
    }
    
    public struct NetworkMessageHeader : INetworkSerializable
    {
        public ulong NetworkObjectId;
        public ushort VarId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializePacked(ref NetworkObjectId);
            serializer.SerializePacked(ref VarId);
        }
    }
}