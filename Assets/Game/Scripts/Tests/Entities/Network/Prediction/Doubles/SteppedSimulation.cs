using System.Collections.Generic;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.Entities.Network.Prediction.Doubles
{
    public sealed class SteppedSimulation<TState> : IReadonlySimulation<TState>, IRestorable<TState> where TState : struct
    {
        public IReadonlyProperty<TState> State => _state;
        public TState Previous { get; private set; }
        public ISource<TState> Stepped => _stepped;
        public IReadOnlyList<TState> Applied => _applied;

        private readonly ViewableProperty<TState> _state;
        private readonly Signal<TState> _stepped = new();
        private readonly List<TState> _applied = new();

        public SteppedSimulation(TState initial = default)
        {
            _state = new ViewableProperty<TState>(initial);
            Previous = initial;
        }

        public void Step(TState next)
        {
            Previous = _state.Value;
            _state.Value = next;
            _stepped.Fire(next);
        }

        public void Apply(TState state)
        {
            _applied.Add(state);
            Previous = state;
            _state.Value = state;
        }
    }
}
