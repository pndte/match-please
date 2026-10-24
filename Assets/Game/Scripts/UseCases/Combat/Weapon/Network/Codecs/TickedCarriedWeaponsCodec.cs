using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Serialization;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Unity.Netcode;

namespace Bw.UseCases.Combat.Weapon.Network.Codecs
{
    public struct TickedCarriedWeaponsCodec : ICodec<TickedState<CarriedWeapons>>
    {
        public TickedState<CarriedWeapons> Value
        {
            get => _value;
            set => _value = value;
        }

        private TickedState<CarriedWeapons> _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var tick = Value.Tick;
            var count = serializer.IsWriter ? Value.State.Weapons.Length : 0;
            serializer.SerializePacked(ref tick);
            serializer.SerializePacked(ref count);

            var weapons = serializer.IsWriter ? Value.State.Weapons : count == 0 ? Array.Empty<CarriedWeapon>() : new CarriedWeapon[count];
            for (var index = 0; index < count; index++)
            {
                var weapon = weapons[index].Weapon;
                var slot = weapons[index].Slot;
                serializer.SerializePacked(ref weapon);
                serializer.SerializeValue(ref slot);
                weapons[index] = new CarriedWeapon(weapon, slot);
            }

            Value = new TickedState<CarriedWeapons>(tick, new CarriedWeapons(weapons));
        }
    }
}
