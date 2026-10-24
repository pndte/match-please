using JetBrains.Collections.Viewable;

namespace Bw.Entities.Simulation
{
    public interface IReadonlySimulation<TState> where TState : struct
    {
        public IReadonlyProperty<TState> State { get; }
        public TState Previous { get; }
        public ISource<TState> Stepped { get; }
    }
}
