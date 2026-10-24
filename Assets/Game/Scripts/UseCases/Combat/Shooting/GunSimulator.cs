using System;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Shooting.Extensions;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Combat.Weapon.Extensions;
using UnityEngine;

namespace Bw.UseCases.Combat.Shooting
{
    public sealed class GunSimulator : IWeaponSimulator<GunState>
    {
        private readonly ISimulationStep _step;
        private readonly ShootingWeaponConfig _config;

        public GunSimulator(ISimulationStep step, ShootingWeaponConfig config)
        {
            _step = step;
            _config = config;
        }

        public GunState Step(GunState state, WeaponInput input)
        {
            var maxAmmo = _config.AmmoSettings.Max;
            var ammo = state.Ammo;
            var cooldown = Math.Max(0, state.CooldownTicks - 1);
            var reload = state.ReloadTicks;
            var shots = state.Shots;
            var aim = state.Aim.TurnedTowards(input.Aim, _config.TurnSpeed * _step.Duration);

            if (reload > 0)
            {
                reload--;
                if (reload == 0)
                    ammo = maxAmmo;
            }
            else if ((input.ReloadPressed && ammo < maxAmmo) || (input.Pulled && ammo == 0))
            {
                reload = Math.Max(1, Ticks(_config.ReloadTime));
            }

            if (input.Pulled && reload == 0 && cooldown == 0 && ammo > 0)
            {
                ammo--;
                cooldown = Ticks(_config.ShootCooldown);
                shots++;
            }

            return new GunState(aim, ammo, cooldown, reload, shots);
        }

        public GunState Interrupted(GunState state) =>
            state.WithoutReload();

        public GunState Rested(GunState state) =>
            Interrupted(state).CooledDown();

        private int Ticks(float seconds) =>
            Mathf.CeilToInt(seconds / _step.Duration);
    }
}
