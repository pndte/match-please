using System.Collections.Generic;
using Bw.Entities.Simulation;

namespace Bw.Tests.Entities.Network.Prediction.Doubles
{
    public sealed class RecordingView<TState> : IStateView<TState> where TState : struct
    {
        public IReadOnlyList<(TState From, TState To, float Progress)> Shown => _shown;

        private readonly List<(TState, TState, float)> _shown = new();

        public void Show(TState from, TState to, float progress) =>
            _shown.Add((from, to, progress));
    }
}
