using Bw.UseCases.Character;
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
            IHitConfig config)
        {
            weapon.Fired.Advise(lifetime, shot =>
                shots.Submit(new RaycastShot(tracer, shot, Hit.Along(tracer.Aim(shot.Aim).Direction, config), weapon)));
        }
    }
}
