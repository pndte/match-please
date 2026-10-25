using System.Collections.Generic;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.Doubles
{
    public sealed class RestorableValue<TState> : IRestorable<TState> where TState : struct
    {
        public IReadonlyProperty<TState> State => _state;
        public IReadOnlyList<TState> Applied => _applied;

        private readonly ViewableProperty<TState> _state;
        private readonly List<TState> _applied = new();

        public RestorableValue(TState initial = default)
        {
            _state = new ViewableProperty<TState>(initial);
        }

        public void Apply(TState state)
        {
            _applied.Add(state);
            _state.Value = state;
        }

        public void Simulate(TState state) =>
            _state.Value = state;
    }
}
