using System;
using Bw.Entities.Simulation;
using Bw.UseCases.Props.Extensions;
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
            return state.Phase switch
            {
                PropPhase.Holding => FromHolding(state, input, aim),
                PropPhase.WindingUp => FromWindingUp(state, input, aim),
                _ => throw new ArgumentOutOfRangeException(nameof(state), state.Phase, "A held prop has no such phase.")
            };
        }

        public PropState Interrupted(PropState state) =>
            new(state.Aim, PropPhase.Holding, 0, state.Throws, true);

        private static PropState FromHolding(PropState state, WeaponInput input, float aim) =>
            input.Trigger && !state.TriggerHeld
                ? new PropState(aim, PropPhase.WindingUp, 1, state.Throws, true)
                : new PropState(aim, PropPhase.Holding, 0, state.Throws, input.Trigger);

        private PropState FromWindingUp(PropState state, WeaponInput input, float aim) =>
            state.PhaseTicks < _config.WindUpTicks(_step)
                ? new PropState(aim, PropPhase.WindingUp, state.PhaseTicks + 1, state.Throws, input.Trigger)
                : new PropState(aim, PropPhase.Holding, 0, state.Throws + 1, input.Trigger);
    }
}
