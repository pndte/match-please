using Bw.Entities.Simulation;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Abstractions.Hands;

namespace Bw.UseCases.Character.Network
{
    public sealed class RootedCharacterSteps : ISteppable<CharacterControls>
    {
        private readonly ISteppable<CharacterControls> _steps;
        private readonly IReadonlyHands _hands;

        public RootedCharacterSteps(ISteppable<CharacterControls> steps, IReadonlyHands hands)
        {
            _steps = steps;
            _hands = hands;
        }

        public void Step(CharacterControls controls) =>
            _steps.Step(Rooted ? new CharacterControls(default, controls.Hands, controls.Guard) : controls);

        private bool Rooted =>
            _hands.TryFindInHands(out var inHands) && inHands.Rooting;
    }
}
