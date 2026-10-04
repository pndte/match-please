using JetBrains.Lifetimes;

namespace Bw.Entities.Simulation
{
    public sealed class SimulationViewSync<TState> where TState : struct
    {
        public SimulationViewSync(Lifetime lifetime, IReadonlySimulation<TState> simulation, IStateView<TState> view)
        {
            simulation.State.Advise(lifetime, state => view.Show(state, state, 0f));
        }
    }
}
