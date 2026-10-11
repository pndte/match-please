using Bw.Entities.Network;
using Bw.UseCases.Shooting.Weapon.Network.Requests;
using Unity.Netcode;

namespace Bw.UseCases.Shooting.Weapon.Network.Codecs
{
    public struct WeaponPickupCodec : ICodec<WeaponPickup>
    {
        public WeaponPickup Value
        {
            get => _value;
            set => _value = value;
        }

        private WeaponPickup _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var point = Value.Point;
            serializer.SerializeValue(ref point);
            Value = new WeaponPickup(point);
        }
    }
}
