using System;
using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.Melee;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.EditorTools.Shooting
{
    public abstract class PreviewShooting
    {
        private PreviewShooting()
        {
        }

        public abstract WeaponState Initial { get; }

        public static PreviewShooting Gun(ShootingWeaponConfig config) =>
            new GunShooting(config, new Spread(config));

        public static PreviewShooting Melee(MeleeWeaponConfig config) =>
            new MeleeShooting(config);

        public abstract IWeaponSimulator SimulatorFor(ISimulationStep step);

        public abstract bool Accepted(WeaponState before, WeaponState after);

        public abstract float ThrowOf(float aim, float roll);

        public abstract TResult Switch<TResult>(Func<ShootingWeaponConfig, TResult> gun, Func<MeleeWeaponConfig, TResult> melee);

        private sealed class GunShooting : PreviewShooting
        {
            private readonly ShootingWeaponConfig _config;
            private readonly ISpread _spread;

            public GunShooting(ShootingWeaponConfig config, ISpread spread)
            {
                _config = config;
                _spread = spread;
            }

            public override WeaponState Initial =>
                new(0f, _config.AmmoSettings.Max, 0, 0, 0);

            public override IWeaponSimulator SimulatorFor(ISimulationStep step) =>
                new WeaponSimulator(step, _config);

            public override bool Accepted(WeaponState before, WeaponState after) =>
                after.Shots != before.Shots;

            public override float ThrowOf(float aim, float roll) =>
                _spread.ThrowOf(aim, roll);

            public override TResult Switch<TResult>(Func<ShootingWeaponConfig, TResult> gun, Func<MeleeWeaponConfig, TResult> melee) =>
                gun(_config);
        }

        private sealed class MeleeShooting : PreviewShooting
        {
            private readonly MeleeWeaponConfig _config;

            public MeleeShooting(MeleeWeaponConfig config)
            {
                _config = config;
            }

            public override WeaponState Initial =>
                default;

            public override IWeaponSimulator SimulatorFor(ISimulationStep step) =>
                new MeleeSimulator(step, _config);

            public override bool Accepted(WeaponState before, WeaponState after) =>
                after.CooldownTicks > before.CooldownTicks;

            public override float ThrowOf(float aim, float roll) =>
                0f;

            public override TResult Switch<TResult>(Func<ShootingWeaponConfig, TResult> gun, Func<MeleeWeaponConfig, TResult> melee) =>
                melee(_config);
        }
    }
}
