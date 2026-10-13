using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Replication
{
    public sealed class PassiveStepper
    {
        public PassiveStepper(Lifetime lifetime, INetworkTicks ticks, ISteppable simulation)
        {
            ticks.Ticked(TickPhase.Passive).Advise(lifetime, _ => simulation.Step());
        }
    }
}
