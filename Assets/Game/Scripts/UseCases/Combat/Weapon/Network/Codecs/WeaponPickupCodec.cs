using Bw.Entities.Network;
using Bw.Entities.Network.Serialization;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Unity.Netcode;

namespace Bw.UseCases.Combat.Weapon.Network.Codecs
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
            var weapon = Value.Weapon;
            serializer.SerializePacked(ref weapon);
            Value = new WeaponPickup(weapon);
        }
    }
}
