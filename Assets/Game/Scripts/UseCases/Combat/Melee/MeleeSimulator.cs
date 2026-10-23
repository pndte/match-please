using System;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Melee.Extensions;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Combat.Weapon.Extensions;

namespace Bw.UseCases.Combat.Melee
{
    public sealed class MeleeSimulator : IWeaponSimulator<MeleeState>
    {
        private readonly ISimulationStep _step;
        private readonly MeleeWeaponConfig _config;

        public MeleeSimulator(ISimulationStep step, MeleeWeaponConfig config)
        {
            _step = step;
            _config = config;
        }

        public MeleeState Step(MeleeState state, WeaponInput input)
        {
            var next = state.Phase switch
            {
                MeleePhase.Idle => FromIdle(state, input),
                MeleePhase.Jab => FromJab(state, input),
                MeleePhase.Charging => FromCharging(state, input),
                MeleePhase.StrongSwing => FromStrongSwing(state, input),
                _ => throw new ArgumentOutOfRangeException(nameof(state), state.Phase, "The fists have no such phase.")
            };
            var aim = state.Aim.TurnedTowards(input.Aim, _config.TurnSpeed * _step.Duration);
            return new MeleeState(aim, next.Phase, next.PhaseTicks, next.Held, next.ChargeTicks, Struck(next) ? next.Shots + 1 : next.Shots);
        }

        public MeleeState Interrupted(MeleeState state) =>
            Idle(state);

        public MeleeState Rested(MeleeState state) =>
            Interrupted(state);

        private static MeleeState FromIdle(MeleeState state, WeaponInput input) =>
            input.Pulled ? Jab(state) : state;

        private MeleeState FromJab(MeleeState state, WeaponInput input)
        {
            var ticks = state.PhaseTicks + 1;
            var held = state.Held && input.Pulled;
            if (ticks < _config.SwingTicks(_step))
                return In(state, MeleePhase.Jab, ticks, held, 0);

            if (held)
                return In(state, MeleePhase.Charging, 0, false, 0);

            return input.Pulled ? Jab(state) : Idle(state);
        }

        private MeleeState FromCharging(MeleeState state, WeaponInput input)
        {
            var charged = state.ChargeTicks + 1;
            return Released(input, charged)
                ? In(state, MeleePhase.StrongSwing, 0, false, charged)
                : In(state, MeleePhase.Charging, charged, false, charged);
        }

        private MeleeState FromStrongSwing(MeleeState state, WeaponInput input)
        {
            var ticks = state.PhaseTicks + 1;
            if (ticks < _config.StrongSwingTicks(_step))
                return In(state, MeleePhase.StrongSwing, ticks, false, state.ChargeTicks);

            return input.Pulled ? Jab(state) : Idle(state);
        }

        private bool Released(WeaponInput input, int chargedTicks) =>
            (chargedTicks >= _config.MinChargeTicks(_step) && !input.Pulled)
            || chargedTicks >= _config.FullChargeTicks(_step) + _config.FullChargeHoldTicks(_step);

        private bool Struck(MeleeState state) =>
            state.Phase switch
            {
                MeleePhase.Jab => state.PhaseTicks == _config.StrikeTicks(_step),
                MeleePhase.StrongSwing => state.PhaseTicks == _config.StrongStrikeTicks(_step),
                _ => false
            };

        private static MeleeState Jab(MeleeState state) =>
            In(state, MeleePhase.Jab, 0, true, 0);

        private static MeleeState Idle(MeleeState state) =>
            In(state, MeleePhase.Idle, 0, false, 0);

        private static MeleeState In(
            MeleeState state,
            MeleePhase phase,
            int phaseTicks,
            bool held,
            int chargeTicks) =>
            new(state.Aim, phase, phaseTicks, held, chargeTicks, state.Shots);
    }
}
