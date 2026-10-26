using Bw.Entities.Simulation;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Hands.Network;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Movement;

namespace Bw.UseCases.Character.Network
{
    public sealed class CharacterStepper : ISteppable<CharacterControls>
    {
        private readonly CharacterTrigger _trigger;
        private readonly ISteppable<MovementInput> _movement;
        private readonly ISteppable<HandsInput> _hands;
        private readonly MovementControlsForm _movementForm;
        private readonly HandsControlsForm _handsForm;

        public CharacterStepper(
            CharacterTrigger trigger,
            ISteppable<MovementInput> movement,
            ISteppable<HandsInput> hands,
            MovementControlsForm movementForm,
            HandsControlsForm handsForm)
        {
            _trigger = trigger;
            _movement = movement;
            _hands = hands;
            _movementForm = movementForm;
            _handsForm = handsForm;
        }

        public void Step(CharacterControls controls)
        {
            _trigger.Take(controls.Hands.Trigger);
            _movement.Step(_movementForm.InputOf(controls));
            _hands.Step(_handsForm.InputOf(controls.Hands, controls.Guard.Raised));
        }
    }
}
