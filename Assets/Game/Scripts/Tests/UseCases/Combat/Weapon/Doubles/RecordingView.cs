using System.Collections.Generic;
using Bw.Entities.Simulation;

namespace Bw.Tests.UseCases.Combat.Weapon.Doubles
{
    public sealed class RecordingView<TState> : IStateView<TState> where TState : struct
    {
        public IReadOnlyList<(TState from, TState to, float progress)> Shown => _shown;

        private readonly List<(TState from, TState to, float progress)> _shown = new();

        public void Show(TState from, TState to, float progress) =>
            _shown.Add((from, to, progress));
    }
}
