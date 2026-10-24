using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Mortality;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public readonly struct RaycastShot //TODO: wtf is this
    {
        public readonly IShotTracer Tracer;
        public readonly WeaponShot WeaponShot;
        public readonly ShotTiming Timing;
        public readonly Hit Hit;
        public readonly IReadonlyWeapon Weapon;
        public readonly IReadonlyCharacter Shooter;

        public RaycastShot(
            IShotTracer tracer,
            TimedShot shot,
            Hit hit,
            IReadonlyWeapon weapon)
        {
            Tracer = tracer;
            WeaponShot = shot.Shot;
            Timing = shot.Timing;
            Hit = hit;
            Weapon = weapon;
            Shooter = shot.Shooter;
        }
    }
}
