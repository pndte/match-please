using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting
{
    public sealed class RaycastShooter
    {
        public RaycastShooter(
            Lifetime lifetime,
            IReadonlyWeapon weapon,
            IShotTracer tracer,
            IRaycastShots shots,
            ShootingWeaponConfig config)
        {
            weapon.Fired.Advise(lifetime, shot => shots.Submit(new RaycastShot(tracer, shot, config.Damage, weapon)));
        }
    }
}
