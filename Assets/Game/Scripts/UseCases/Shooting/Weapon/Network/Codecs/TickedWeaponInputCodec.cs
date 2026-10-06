using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Requests;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.UseCases.Shooting.Weapon.Network.Codecs
{
    public struct TickedWeaponInputCodec : ICodec<TickedInput<WeaponInput>>
    {
        public TickedInput<WeaponInput> Value
        {
            get => _value;
            set => _value = value;
        }

        private TickedInput<WeaponInput> _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var tick = Value.Tick;
            var aim = Value.Input.Aim;
            var trigger = Value.Input.Trigger;
            var reload = Value.Input.Reload;
            var viewDelay = Value.Input.ViewDelay;
            var previousAim = Value.PreviousInput.Aim;
            var previousTrigger = Value.PreviousInput.Trigger;
            var previousReload = Value.PreviousInput.Reload;
            var previousViewDelay = Value.PreviousInput.ViewDelay;

            serializer.SerializePacked(ref tick);
            serializer.SerializeValue(ref aim);
            serializer.SerializeValue(ref trigger);
            serializer.SerializeValue(ref reload);
            if (trigger)
                serializer.SerializeValue(ref viewDelay);
            serializer.SerializeValue(ref previousAim);
            serializer.SerializeValue(ref previousTrigger);
            serializer.SerializeValue(ref previousReload);
            if (previousTrigger)
                serializer.SerializeValue(ref previousViewDelay);

            Value = new TickedInput<WeaponInput>(
                tick,
                new WeaponInput(aim, trigger, reload, viewDelay),
                new WeaponInput(previousAim, previousTrigger, previousReload, previousViewDelay));
        }
    }
}
