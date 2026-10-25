using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.Entities.Network.Doubles
{
    public sealed class SimulationValue<TState> : IReadonlySimulation<TState> where TState : struct
    {
        public IReadonlyProperty<TState> State => _state;
        public TState Previous { get; private set; }
        public ISource<TState> Stepped => _stepped;

        private readonly ViewableProperty<TState> _state;
        private readonly Signal<TState> _stepped = new();

        public SimulationValue(TState initial = default)
        {
            _state = new ViewableProperty<TState>(initial);
            Previous = initial;
        }

        public void Step(TState state)
        {
            Previous = _state.Value;
            _state.Value = state;
            _stepped.Fire(state);
        }
    }
}
