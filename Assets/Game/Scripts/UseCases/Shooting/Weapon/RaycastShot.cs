using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Shooting.Weapon
{
    public readonly struct RaycastShot
    {
        public readonly IShotTracer Tracer;
        public readonly WeaponShot Shot;
        public readonly float Damage;

        public RaycastShot(IShotTracer tracer, WeaponShot shot, float damage)
        {
            Tracer = tracer;
            Shot = shot;
            Damage = damage;
        }
    }
}
