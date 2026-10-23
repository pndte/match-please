using Bw.Entities.Simulation;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Movement;

namespace Bw.UseCases.Character.Network
{
    public sealed class ControlledMovement : ISimulator<CharacterControls, MovementState>
    {
        private readonly ISimulator<MovementInput, MovementState> _motor;
        private readonly MovementControlsForm _form;

        public ControlledMovement(ISimulator<MovementInput, MovementState> motor, MovementControlsForm form)
        {
            _motor = motor;
            _form = form;
        }

        public MovementState Step(MovementState state, CharacterControls controls) =>
            _motor.Step(state, _form.InputOf(controls));
    }
}
