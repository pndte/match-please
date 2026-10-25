using Bw.Entities.Network.Prediction.Stream;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Movement.Network.Requests;

namespace Bw.UseCases.Character.Network
{
    public sealed class CharacterControlsDivergence : IInputDivergence<CharacterControls>
    {
        public bool Differs(CharacterControls guessed, CharacterControls relayed) =>
            Differs(guessed.Movement, relayed.Movement)
            || Differs(guessed.Hands, relayed.Hands)
            || guessed.Guard.Raised != relayed.Guard.Raised
            || guessed.Guard.Raised && !guessed.Guard.Aim.Equals(relayed.Guard.Aim);

        private static bool Differs(MovementControls guessed, MovementControls relayed) =>
            !guessed.Horizontal.Equals(relayed.Horizontal)
            || guessed.JumpPressed != relayed.JumpPressed
            || guessed.JumpHeld != relayed.JumpHeld
            || guessed.Down != relayed.Down;

        private static bool Differs(HandsControls guessed, HandsControls relayed) =>
            !guessed.Aim.Equals(relayed.Aim)
            || guessed.Trigger.Pulled != relayed.Trigger.Pulled
            || guessed.Trigger.Press.Number != relayed.Trigger.Press.Number
            || guessed.Trigger.Press.Age != relayed.Trigger.Press.Age
            || guessed.ReloadPressed != relayed.ReloadPressed
            || !guessed.Choice.Equals(relayed.Choice);
    }
}
