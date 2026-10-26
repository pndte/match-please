using Bw.Entities.Network.Prediction.Stream;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon.Network.Extensions;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Movement.Network.Requests;

namespace Bw.UseCases.Character.Network
{
    public sealed class CharacterControlsContinuation : IInputContinuation<CharacterControls>
    {
        public CharacterControls Continued(CharacterControls last, int ticks)
        {
            var movement = last.Movement;
            var hands = last.Hands;
            return new CharacterControls(
                new MovementControls(movement.Horizontal, false, movement.JumpHeld, false),
                new HandsControls(hands.Aim, Aged(hands.Trigger.WithoutFreshPress(), ticks), false, DrawControl.Keep),
                last.Guard);
        }

        private static TriggerControls Aged(TriggerControls trigger, int ticks) =>
            trigger.WithPress(trigger.Press.WithAge(trigger.Press.Age + ticks));
    }
}
