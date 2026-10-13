using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;

namespace Bw.UseCases.Props
{
    public sealed class PropSimulator : IWeaponSimulator<PropState>
    {
        private readonly ISimulationStep _step;
        private readonly HeldPropConfig _config;

        public PropSimulator(ISimulationStep step, HeldPropConfig config)
        {
            _step = step;
            _config = config;
        }

        public PropState Step(PropState state, WeaponInput input)
        {
            var aim = state.Aim.TurnedTowards(input.Aim, _config.TurnSpeed * _step.Duration);
            var pressed = input.Trigger && !state.TriggerHeld;
            return new PropState(aim, pressed ? state.Throws + 1 : state.Throws, input.Trigger);
        }

        public PropState Interrupted(PropState state) =>
            new(state.Aim, state.Throws, true);
    }
}
