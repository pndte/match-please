using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.LagCompensation
{
    public sealed class SimulationHistory<TState> : TickHistory<TState> where TState : struct
    {
        public SimulationHistory(
            Lifetime lifetime,
            IReadonlySimulation<TState> simulation,
            INetworkTicks ticks,
            IWorldStep worldStep,
            LagCompensationConfig config)
            : base(config.RewindTicks(ticks) + 2)
        {
            worldStep.Stepped(TickPhase.Publish).Advise(lifetime, tick => Record(tick, simulation.State.Value));
        }
    }
}
