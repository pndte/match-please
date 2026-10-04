using Bw.Entities;
using Unity.Netcode;

namespace Bw.Entities.Network.Codecs
{
    public struct ClientConnectionStateCodec : ICodec<ClientConnectionState>
    {
        public ClientConnectionState Value
        {
            get => _value;
            set => _value = value;
        }

        private ClientConnectionState _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var raw = (int)_value;
            serializer.SerializeValue(ref raw);
            _value = (ClientConnectionState)raw;
        }
    }
}
