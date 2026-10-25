using System.Collections.Generic;
using Bw.Entities.Simulation;

namespace Bw.Tests.UseCases.Movement.Doubles
{
    public sealed class ShownStates<TState> : IStateView<TState> where TState : struct
    {
        public IReadOnlyList<(TState From, TState To, float Progress)> Shown => _shown;
        public (TState From, TState To, float Progress) Last => _shown[_shown.Count - 1];

        private readonly List<(TState From, TState To, float Progress)> _shown = new();

        public void Show(TState from, TState to, float progress) =>
            _shown.Add((from, to, progress));
    }
}
