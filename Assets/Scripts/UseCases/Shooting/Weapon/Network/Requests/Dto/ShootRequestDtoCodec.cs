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
            var clientId = _value.RequestId;
            var target = _value.TargetPosition;
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref target);
            
            _value = new ShootRequestDto(clientId, target);
        }
    }
}
