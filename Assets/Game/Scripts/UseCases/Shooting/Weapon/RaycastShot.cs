using Bw.UseCases.Character;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Shooting.Weapon
{
    public readonly struct RaycastShot
    {
        public readonly IShotTracer Tracer;
        public readonly WeaponShot WeaponShot;
        public readonly Hit Hit;
        public readonly IReadonlyWeapon Weapon;

        public RaycastShot(IShotTracer tracer, WeaponShot weaponShot, Hit hit, IReadonlyWeapon weapon)
        {
            Tracer = tracer;
            WeaponShot = weaponShot;
            Hit = hit;
            Weapon = weapon;
        }
    }
}
