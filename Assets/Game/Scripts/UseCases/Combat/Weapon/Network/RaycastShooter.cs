using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class RaycastShooter<TState> where TState : struct
    {
        public RaycastShooter( //TODO: чрезвычайное к-во параметров
            Lifetime lifetime,
            IReadonlyWeapon weapon,
            IReadonlySimulation<TState> simulation,
            IShotTracer tracer,
            IRaycastShots shots,
            IShotPower<TState> power,
            ShotTimer timer)
        {
            timer.Timed.Advise(lifetime, shot =>
                shots.Submit(new RaycastShot(tracer, shot, power.Along(tracer.Aim(shot.Shot.Aim).Direction, simulation.State.Value), weapon)));
        }
    }
}
