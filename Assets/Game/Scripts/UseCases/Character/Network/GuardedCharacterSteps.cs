using Bw.Entities.Simulation;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Mortality.Guarding;

namespace Bw.UseCases.Character.Network
{
    public sealed class GuardedCharacterSteps : ISteppable<CharacterControls>
    {
        private readonly ISteppable<CharacterControls> _steps;
        private readonly ISteppable<GuardStance> _guard;

        public GuardedCharacterSteps(ISteppable<CharacterControls> steps, ISteppable<GuardStance> guard)
        {
            _steps = steps;
            _guard = guard;
        }

        public void Step(CharacterControls controls)
        {
            _guard.Step(controls.Guard);
            _steps.Step(controls);
        }
    }
}
