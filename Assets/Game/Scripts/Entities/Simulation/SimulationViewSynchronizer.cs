using JetBrains.Lifetimes;

namespace Bw.Entities.Simulation
{
    public sealed class SimulationViewSynchronizer<TState> where TState : struct
    {
        public SimulationViewSynchronizer(Lifetime lifetime, IReadonlySimulation<TState> simulation, IStateView<TState> view)
        {
            simulation.State.Advise(lifetime, state => view.Show(state, state, 0f));
        }
    }
}
