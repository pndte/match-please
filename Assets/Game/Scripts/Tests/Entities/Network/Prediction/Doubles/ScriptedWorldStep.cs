using System.Collections.Generic;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.Entities.Network.Prediction.Doubles
{
    public sealed class ScriptedWorldStep : IWorldStep
    {
        public IReadonlyProperty<bool> Replaying => _replaying;

        private readonly ViewableProperty<bool> _replaying = new(false);
        private readonly Dictionary<TickPhase, Signal<int>> _phases = new();

        public ISource<int> Stepped(TickPhase phase) =>
            SignalOf(phase);

        public void Fire(TickPhase phase, int tick) =>
            SignalOf(phase).Fire(tick);

        public void SetReplaying(bool replaying) =>
            _replaying.Value = replaying;

        private Signal<int> SignalOf(TickPhase phase)
        {
            if (!_phases.TryGetValue(phase, out var signal))
            {
                signal = new Signal<int>();
                _phases.Add(phase, signal);
            }

            return signal;
        }
    }
}
