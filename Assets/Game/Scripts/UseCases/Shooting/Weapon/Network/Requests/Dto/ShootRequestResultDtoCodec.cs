using Bw.Entities.Network;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public struct ShootRequestResultDtoCodec : ICodec<ShootRequestResultDto>
    {
        public ShootRequestResultDto Value
        {
            get => _value;
            set => _value = value;
        }

        private ShootRequestResultDto _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var requestId = Value.RequestId;
            var position = Value.TargetPosition;
            var accepted = Value.Accepted;

            serializer.SerializePacked(ref requestId);
            serializer.SerializeValue(ref position);
            serializer.SerializePacked(ref accepted);

            Value = new ShootRequestResultDto(requestId, position, accepted);
        }
    }
}
