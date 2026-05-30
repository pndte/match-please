using Bw.Entities.Network;
using Unity.Netcode;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public struct ShootRequestDtoCodec : ICodec<ShootRequestDto>
    {
        public ShootRequestDto Value
        {
            get => _value;
            set => _value = value;
        }

        private ShootRequestDto _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var requestId = Value.RequestId;
            var position = Value.TargetPosition;
            
            serializer.SerializeValue(ref requestId);
            serializer.SerializeValue(ref position);
            
            Value = new ShootRequestDto(requestId, position);
        }
    }
}
