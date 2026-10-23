using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Mortality;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public readonly struct RaycastShot
    {
        public readonly IShotTracer Tracer;
        public readonly WeaponShot WeaponShot;
        public readonly ShotTiming Timing;
        public readonly Hit Hit;
        public readonly IReadonlyWeapon Weapon;

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
        }
    }
}
