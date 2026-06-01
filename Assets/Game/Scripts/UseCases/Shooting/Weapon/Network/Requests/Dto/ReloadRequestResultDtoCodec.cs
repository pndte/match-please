using Bw.Entities.Network;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public struct ReloadRequestResultDtoCodec : ICodec<ReloadRequestResultDto>
    {
        public ReloadRequestResultDto Value
        {
            get => _value;
            set => _value = value;
        }

        private ReloadRequestResultDto _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var requestId = Value.RequestId;
            var accepted = Value.Accepted;

            serializer.SerializePacked(ref requestId);
            serializer.SerializePacked(ref accepted);

            Value = new ReloadRequestResultDto(requestId, accepted);
        }
    }
}
