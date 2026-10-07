using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.UseCases.Shooting.Weapon.Network.Codecs
{
    public struct TickedWeaponStateCodec : ICodec<TickedState<WeaponState>>
    {
        public TickedState<WeaponState> Value
        {
            get => _value;
            set => _value = value;
        }

        private TickedState<WeaponState> _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var tick = Value.Tick;
            var aim = Value.State.Aim;
            var ammo = Value.State.Ammo;
            var cooldownTicks = Value.State.CooldownTicks;
            var reloadTicks = Value.State.ReloadTicks;
            var shots = Value.State.Shots;

            serializer.SerializePacked(ref tick);
            serializer.SerializeValue(ref aim); //TODO: квантование
            serializer.SerializePacked(ref ammo);
            serializer.SerializePacked(ref cooldownTicks);
            serializer.SerializePacked(ref reloadTicks);
            serializer.SerializePacked(ref shots);

            Value = new TickedState<WeaponState>(
                tick,
                new WeaponState(aim, ammo, cooldownTicks, reloadTicks, shots));
        }
    }
}
