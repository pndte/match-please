using Bw.Entities.Network;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public struct ReloadRequestDtoCodec : ICodec<ReloadRequestDto>
    {
        public ReloadRequestDto Value
        {
            get => _value;
            set => _value = value;
        }

        private ReloadRequestDto _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var requestId = Value.RequestId;

            serializer.SerializePacked(ref requestId);

            Value = new ReloadRequestDto(requestId);
        }
    }
}
