using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Rollback
{
    public sealed class PhysicsSchedule
    {
        public PhysicsSchedule(Lifetime lifetime, ISteppable physics, IWorldStep world)
        {
            world.Stepped(TickPhase.Physics).Advise(lifetime, _ => physics.Step());
        }
    }
}
