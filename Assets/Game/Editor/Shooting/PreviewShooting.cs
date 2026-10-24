using System;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Melee;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Shooting.Abstractions;

namespace Bw.EditorTools.Shooting
{
    public abstract class PreviewShooting
    {
        private PreviewShooting()
        {
        }

        public static PreviewShooting Gun(ShootingWeaponConfig config) =>
            new GunShooting(config, new Spread(config));

        public static PreviewShooting Melee(MeleeWeaponConfig config) =>
            new MeleeShooting(config);

        public abstract IPreviewSimulation SimulationFor(ISimulationStep step);

        public abstract float ThrowOf(float aim, float roll);

        public abstract float Returned(float deflection, float seconds);

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

            public override IPreviewSimulation SimulationFor(ISimulationStep step) =>
                new PreviewSimulation<GunState>(
                    new GunSimulator(step, _config),
                    new GunState(0f, _config.AmmoSettings.Max, 0, 0, 0),
                    (before, after) => after.Shots != before.Shots);

            public override float ThrowOf(float aim, float roll) =>
                _spread.ThrowOf(aim, roll);

            public override float Returned(float deflection, float seconds) =>
                _spread.Returned(deflection, seconds);

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

            public override IPreviewSimulation SimulationFor(ISimulationStep step) =>
                new PreviewSimulation<MeleeState>(
                    new MeleeSimulator(step, _config),
                    default,
                    (_, after) => after.PhaseTicks == 0 && (after.Phase == MeleePhase.Jab || after.Phase == MeleePhase.StrongSwing));

            public override float ThrowOf(float aim, float roll) =>
                0f;

            public override float Returned(float deflection, float seconds) =>
                0f;

            public override TResult Switch<TResult>(Func<ShootingWeaponConfig, TResult> gun, Func<MeleeWeaponConfig, TResult> melee) =>
                melee(_config);
        }
    }
}
