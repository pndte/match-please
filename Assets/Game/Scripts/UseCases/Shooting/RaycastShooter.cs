using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting
{
    public sealed class RaycastShooter<TState> where TState : struct
    {
        public RaycastShooter(
            Lifetime lifetime,
            IReadonlyWeapon weapon,
            IReadonlySimulation<TState> simulation,
            IShotTracer tracer,
            IRaycastShots shots,
            IShotPower<TState> power)
        {
            weapon.Fired.Advise(lifetime, shot =>
                shots.Submit(new RaycastShot(tracer, shot, power.Along(tracer.Aim(shot.Aim).Direction, simulation.State.Value), weapon)));
        }
    }
}
