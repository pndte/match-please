using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Movement;

namespace Bw.UseCases.Character.Network
{
    public sealed class MovementControlsForm
    {
        private readonly GuardConfig _guard;

        public MovementControlsForm(GuardConfig guard)
        {
            _guard = guard;
        }

        public MovementInput InputOf(CharacterControls controls)
        {
            var movement = controls.Movement;
            return new MovementInput(movement.Horizontal, movement.JumpPressed, movement.JumpHeld, movement.Down, SlowdownOf(controls.Guard));
        }

        private Slowdown SlowdownOf(GuardStance stance) =>
            stance.Raised ? new Slowdown(1f - _guard.SpeedShare, 1f - _guard.JumpHeightShare) : Slowdown.None;
    }
}
