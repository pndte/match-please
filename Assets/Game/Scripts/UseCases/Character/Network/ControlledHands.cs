using Bw.Entities.Simulation;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Hands.Network;

namespace Bw.UseCases.Character.Network
{
    public sealed class ControlledHands : ISimulator<CharacterControls, HandsState>
    {
        private readonly ISimulator<HandsInput, HandsState> _hands;
        private readonly HandsControlsForm _form;

        public ControlledHands(ISimulator<HandsInput, HandsState> hands, HandsControlsForm form)
        {
            _hands = hands;
            _form = form;
        }

        public HandsState Step(HandsState state, CharacterControls controls) =>
            _hands.Step(state, _form.InputOf(controls.Hands, controls.Guard.Raised));
    }
}
