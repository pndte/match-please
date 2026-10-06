using System;
using Bw.Entities.Simulation;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class WeaponSimulator : ISimulator<WeaponInput, WeaponState>
    {
        private readonly ISimulationStep _step;
        private readonly ShootingWeaponConfig _weaponConfig;
        private readonly WeaponRotationConfig _rotationConfig;

        public WeaponSimulator(
            ISimulationStep step,
            ShootingWeaponConfig weaponConfig,
            WeaponRotationConfig rotationConfig)
        {
            _step = step;
            _weaponConfig = weaponConfig;
            _rotationConfig = rotationConfig;
        }

        public WeaponState Step(WeaponState state, WeaponInput input)
        {
            var maxAmmo = _weaponConfig.AmmoSettings.Max;
            var ammo = state.Ammo;
            var cooldown = Math.Max(0, state.CooldownTicks - 1);
            var reload = state.ReloadTicks;
            var shots = state.Shots;

            if (reload > 0)
            {
                reload--;
                if (reload == 0)
                    ammo = maxAmmo;
            }
            else if (input.Reload && ammo < maxAmmo)
            {
                reload = Math.Max(1, Ticks(_weaponConfig.ReloadTime));
            }

            if (input.Trigger && reload == 0 && cooldown == 0 && ammo > 0)
            {
                ammo--;
                cooldown = Ticks(_weaponConfig.ShootCooldown);
                shots++;
            }

            return new WeaponState(Turn(state.Aim, input.Aim), ammo, cooldown, reload, shots);
        }

        private float Turn(float aim, float target)
        {
            var speed = _rotationConfig.RotationSpeed;
            if (speed <= 0f)
                return target;

            return Mathf.DeltaAngle(0f, Mathf.LerpAngle(aim, target, speed * _step.Duration));
        }

        private int Ticks(float seconds) =>
            Mathf.CeilToInt(seconds / _step.Duration);
    }
}
