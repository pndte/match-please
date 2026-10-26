using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Serialization;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon.Network.Extensions;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Movement.Network.Requests;
using Unity.Netcode;

namespace Bw.UseCases.Character.Network.Codecs
{
    public struct TickedCharacterControlsCodec : ICodec<TickedInput<CharacterControls>>
    {
        private static readonly Quantization Axis = new(3);
        private static readonly Quantization Delay = new(8);

        public TickedInput<CharacterControls> Value
        {
            get => _value;
            set => _value = value;
        }

        private TickedInput<CharacterControls> _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var tick = Value.Tick;
            var movement = Value.Input.Movement;
            var hands = Value.Input.Hands;
            var guard = Value.Input.Guard;
            var previousMovement = Value.PreviousInput.Movement;
            var previousHands = Value.PreviousInput.Hands;
            var previousGuard = Value.PreviousInput.Guard;

            var horizontal = movement.Horizontal;
            var jump = movement.JumpPressed;
            var jumpHeld = movement.JumpHeld;
            var down = movement.Down;
            var aim = hands.Aim;
            var pulled = hands.Trigger.Pulled;
            var viewDelay = hands.Trigger.ViewDelay;
            var predictedDelay = hands.Trigger.PredictedDelay;
            var press = hands.Trigger.Press;
            var reload = hands.ReloadPressed;
            var choiceKind = (byte)hands.Choice.Kind;
            var choiceValue = hands.Choice.Value;
            var raised = guard.Raised;
            var guardAim = guard.Aim;

            var previousHorizontal = previousMovement.Horizontal;
            var previousJump = previousMovement.JumpPressed;
            var previousJumpHeld = previousMovement.JumpHeld;
            var previousDown = previousMovement.Down;
            var previousAim = previousHands.Aim;
            var previousPulled = previousHands.Trigger.Pulled;
            var previousViewDelay = previousHands.Trigger.ViewDelay;
            var previousPredictedDelay = previousHands.Trigger.PredictedDelay;
            var previousPress = previousHands.Trigger.Press;
            var previousReload = previousHands.ReloadPressed;
            var previousChoiceKind = (byte)previousHands.Choice.Kind;
            var previousChoiceValue = previousHands.Choice.Value;
            var previousRaised = previousGuard.Raised;
            var previousGuardAim = previousGuard.Aim;

            var samePress = press.Age > 0 && previousPress.Equals(OneTickEarlier(press));
            var choosing = !hands.Choice.Equals(DrawControl.Keep) || !previousHands.Choice.Equals(DrawControl.Keep);

            serializer.SerializePacked(ref tick);
            serializer.SerializeFlags(
                ref jump,
                ref jumpHeld,
                ref down,
                ref pulled,
                ref reload,
                ref raised,
                ref samePress,
                ref choosing);
            serializer.SerializeFlags(
                ref previousJump,
                ref previousJumpHeld,
                ref previousDown,
                ref previousPulled,
                ref previousReload,
                ref previousRaised);
            serializer.SerializeQuantized(ref horizontal, Axis);
            serializer.SerializeQuantized(ref previousHorizontal, Axis);

            serializer.SerializeAngle(ref aim);
            if (pulled)
            {
                serializer.SerializeQuantized(ref viewDelay, Delay);
                serializer.SerializeQuantized(ref predictedDelay, Delay);
            }

            SerializePress(serializer, ref press);
            if (raised)
                serializer.SerializeAngle(ref guardAim);

            serializer.SerializeAngle(ref previousAim);
            if (previousPulled)
            {
                serializer.SerializeQuantized(ref previousViewDelay, Delay);
                serializer.SerializeQuantized(ref previousPredictedDelay, Delay);
            }

            if (samePress)
                previousPress = OneTickEarlier(press);
            else
                SerializePress(serializer, ref previousPress);

            if (previousRaised)
                serializer.SerializeAngle(ref previousGuardAim);

            if (choosing)
            {
                serializer.SerializeValue(ref choiceKind);
                serializer.SerializeValue(ref choiceValue);
                serializer.SerializeValue(ref previousChoiceKind);
                serializer.SerializeValue(ref previousChoiceValue);
            }

            Value = new TickedInput<CharacterControls>(
                tick,
                new CharacterControls(
                    new MovementControls(horizontal, jump, jumpHeld, down),
                    new HandsControls(
                        aim,
                        new TriggerControls(pulled, pulled ? viewDelay : 0f, pulled ? predictedDelay : 0f, press),
                        reload,
                        new DrawControl((DrawControlKind)choiceKind, choiceValue)),
                    new GuardStance(raised, raised ? guardAim : 0f)),
                new CharacterControls(
                    new MovementControls(previousHorizontal, previousJump, previousJumpHeld, previousDown),
                    new HandsControls(
                        previousAim,
                        new TriggerControls(previousPulled, previousPulled ? previousViewDelay : 0f, previousPulled ? previousPredictedDelay : 0f, previousPress),
                        previousReload,
                        new DrawControl((DrawControlKind)previousChoiceKind, previousChoiceValue)),
                    new GuardStance(previousRaised, previousRaised ? previousGuardAim : 0f)));
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
