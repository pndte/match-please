using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Serialization;
using Bw.UseCases.Shooting.Weapon.Extensions;
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
            var press = Value.Input.Press;
            var previousAim = Value.PreviousInput.Aim;
            var previousTrigger = Value.PreviousInput.Trigger;
            var previousReload = Value.PreviousInput.Reload;
            var previousViewDelay = Value.PreviousInput.ViewDelay;
            var previousPredictedDelay = Value.PreviousInput.PredictedDelay;
            var previousPress = Value.PreviousInput.Press;
            var samePress = press.Age > 0 && previousPress.Equals(OneTickEarlier(press));

            serializer.SerializePacked(ref tick);
            serializer.SerializeFlags(ref trigger, ref reload, ref previousTrigger, ref previousReload, ref samePress);
            serializer.SerializeAngle(ref aim);
            if (trigger)
            {
                serializer.SerializeQuantized(ref viewDelay, Delay);
                serializer.SerializeQuantized(ref predictedDelay, Delay);
            }

            SerializePress(serializer, ref press);
            serializer.SerializeAngle(ref previousAim);
            if (previousTrigger)
            {
                serializer.SerializeQuantized(ref previousViewDelay, Delay);
                serializer.SerializeQuantized(ref previousPredictedDelay, Delay);
            }

            if (samePress)
                previousPress = OneTickEarlier(press);
            else
                SerializePress(serializer, ref previousPress);

            Value = new TickedInput<WeaponInput>(
                tick,
                new WeaponInput(aim, trigger, reload, viewDelay, predictedDelay, press),
                new WeaponInput(previousAim, previousTrigger, previousReload, previousViewDelay, previousPredictedDelay, previousPress));
        }

        private static void SerializePress<T>(BufferSerializer<T> serializer, ref TriggerPress press) where T : IReaderWriter
        {
            var number = press.Number;
            var age = press.Age;
            var viewDelay = press.ViewDelay;
            var predictedDelay = press.PredictedDelay;

            serializer.SerializeValue(ref number);
            serializer.SerializeValue(ref age);
            serializer.SerializeQuantized(ref viewDelay, Delay);
            serializer.SerializeQuantized(ref predictedDelay, Delay);
            press = new TriggerPress(number, age, viewDelay, predictedDelay, 0);
        }

        private static TriggerPress OneTickEarlier(TriggerPress press) =>
            press.Age == byte.MaxValue ? press : press.WithAge(press.Age - 1);
    }
}
