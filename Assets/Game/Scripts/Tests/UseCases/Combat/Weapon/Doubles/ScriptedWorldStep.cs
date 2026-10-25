using System.Collections.Generic;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.UseCases.Combat.Weapon.Doubles
{
    public sealed class ScriptedWorldStep : IWorldStep
    {
        private static readonly TickPhase[] Live = { TickPhase.Prepare, TickPhase.Early, TickPhase.Default, TickPhase.Late, TickPhase.Publish };
        private static readonly TickPhase[] Replayed = { TickPhase.Early, TickPhase.Default, TickPhase.Late };

        public IReadonlyProperty<bool> Replaying => _replaying;

        private readonly ViewableProperty<bool> _replaying = new(false);
        private readonly Dictionary<TickPhase, Signal<int>> _phases = new();

        public ISource<int> Stepped(TickPhase phase) =>
            SignalOf(phase);

        public void Fire(TickPhase phase, int tick) =>
            SignalOf(phase).Fire(tick);

        public void Step(int tick)
        {
            foreach (var phase in Live)
                Fire(phase, tick);
        }

        public void Replay(params int[] ticks)
        {
            _replaying.Value = true;
            foreach (var tick in ticks)
                foreach (var phase in Replayed)
                    Fire(phase, tick);

            _replaying.Value = false;
        }

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
