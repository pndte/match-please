using System;
using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.Melee.Extensions;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;

namespace Bw.UseCases.Shooting.Melee
{
    public sealed class MeleeSimulator : IWeaponSimulator
    {
        private readonly ISimulationStep _step;
        private readonly MeleeWeaponConfig _config;

        public MeleeSimulator(ISimulationStep step, MeleeWeaponConfig config)
        {
            _step = step;
            _config = config;
        }

        public WeaponState Step(WeaponState state, WeaponInput input)
        {
            var swingTicks = _config.SwingTicks(_step);
            var cooldown = Math.Max(0, state.CooldownTicks - 1);
            if (input.Trigger && cooldown == 0)
                cooldown = swingTicks;

            var struck = cooldown == swingTicks - _config.StrikeTicks(_step);
            var aim = state.Aim.TurnedTowards(input.Aim, _config.TurnSpeed * _step.Duration);
            return new WeaponState(aim, state.Ammo, cooldown, state.ReloadTicks, struck ? state.Shots + 1 : state.Shots);
        }

        public WeaponState Interrupted(WeaponState state) =>
            state.WithoutCooldown();
    }
}
