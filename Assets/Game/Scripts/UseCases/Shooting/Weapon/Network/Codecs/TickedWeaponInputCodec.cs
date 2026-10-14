using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.UseCases.Shooting.Weapon.Network.Codecs
{
    public struct TickedWeaponInputCodec : ICodec<TickedInput<WeaponInput>>
    {
        private static readonly Quantization Delay = new(8);

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
            var predictedDelay = Value.Input.PredictedDelay;
            var previousAim = Value.PreviousInput.Aim;
            var previousTrigger = Value.PreviousInput.Trigger;
            var previousReload = Value.PreviousInput.Reload;
            var previousViewDelay = Value.PreviousInput.ViewDelay;
            var previousPredictedDelay = Value.PreviousInput.PredictedDelay;

            serializer.SerializePacked(ref tick);
            serializer.SerializeFlags(ref trigger, ref reload, ref previousTrigger, ref previousReload);
            serializer.SerializeAngle(ref aim);
            if (trigger)
            {
                serializer.SerializeQuantized(ref viewDelay, Delay);
                serializer.SerializeQuantized(ref predictedDelay, Delay);
            }

            serializer.SerializeAngle(ref previousAim);
            if (previousTrigger)
            {
                serializer.SerializeQuantized(ref previousViewDelay, Delay);
                serializer.SerializeQuantized(ref previousPredictedDelay, Delay);
            }

            Value = new TickedInput<WeaponInput>(
                tick,
                new WeaponInput(aim, trigger, reload, viewDelay, predictedDelay),
                new WeaponInput(previousAim, previousTrigger, previousReload, previousViewDelay, previousPredictedDelay));
        }
    }
}
