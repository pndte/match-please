using System.Collections.Generic;
using Bw.Entities;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.Entities.Network.Prediction.Doubles
{
    public sealed class AppliedValue<TState> : IAppliedState<TState> where TState : struct
    {
        public TState Current { get; private set; }
        public ISource<TState> Applied => _applied;
        public IReadOnlyList<TState> History => _history;

        private readonly Signal<TState> _applied = new();
        private readonly List<TState> _history = new();

        public AppliedValue(TState initial = default)
        {
            Current = initial;
        }

        public void Apply(TState state)
        {
            _history.Add(state);
            Current = state;
            _applied.Fire(state);
        }
    }
}
