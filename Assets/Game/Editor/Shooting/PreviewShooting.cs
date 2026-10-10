using Bw.Entities.Simulation;
using Bw.UseCases.Shooting;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.EditorTools.Shooting
{
    public sealed class PreviewShooting
    {
        public ShootingWeaponConfig Config { get; }
        public ISpread Spread { get; }
        public WeaponRotationConfig Rotation { get; }

        public WeaponState Initial =>
            new(0f, Config.AmmoSettings.Max, 0, 0, 0);

        private PreviewShooting(ShootingWeaponConfig config, WeaponRotationConfig rotation, ISpread spread)
        {
            Config = config;
            Spread = spread;
            Rotation = rotation;
        }

        public static PreviewShooting Gun(ShootingWeaponConfig config, WeaponRotationConfig rotation) =>
            new(config, rotation, new Spread(config));

        public ISimulator<WeaponInput, WeaponState> SimulatorFor(ISimulationStep step) =>
            new WeaponSimulator(step, Config, Rotation);

        public bool Accepted(WeaponState before, WeaponState after) =>
            after.Shots != before.Shots;
    }
}
