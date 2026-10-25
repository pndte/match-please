using System;
using System.Collections.Generic;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Rollback
{
    public sealed class WorldRollback : IWorldStep, IRollbackParts
    {
        public IReadonlyProperty<bool> Replaying => _replaying;

        private readonly TickPhaseSignals _phases = new();
        private readonly ViewableProperty<bool> _replaying = new(false);
        private readonly List<IRollbackPart> _parts = new();
        private readonly List<IRollbackPart> _reconciled = new();
        private readonly List<IDivergenceSource> _sources = new();
        private readonly WorldRollbackConfig _config;

        public WorldRollback(Lifetime lifetime, IWorldClock clock, WorldRollbackConfig config)
        {
            _config = config;

            clock.Ticked.Advise(lifetime, Tick);
        }

        public ISource<int> Stepped(TickPhase phase) =>
            _phases.Of(phase);

        public void Track(Lifetime lifetime, IRollbackPart part)
        {
            if (_parts.Contains(part))
                throw new InvalidOperationException("A part rolls back once: it is already tracked.");

            _parts.Add(part);
            lifetime.OnTermination(() => _parts.Remove(part));
        }

        public void Watch(Lifetime lifetime, IDivergenceSource source)
        {
            if (_sources.Contains(source))
                throw new InvalidOperationException("A divergence source is watched once: it is already watched.");

            _sources.Add(source);
            lifetime.OnTermination(() => _sources.Remove(source));
        }

        private void Tick(int tick)
        {
            _phases.Fire(TickPhase.Prepare, tick);
            Reconcile(tick);
            Step(tick);
            _phases.Fire(TickPhase.Publish, tick);
        }

        private void Reconcile(int tick) //TODO: расхождение старше MaxReplayTicks не исправляется: истории ввода на такой пересчёт нет, предсказанное остаётся на своём, пока сервер не пришлёт свежий снимок
        {
            var previous = tick - 1;
            _reconciled.Clear();
            _reconciled.AddRange(_parts);
            for (var index = 0; index < _reconciled.Count; index++)
                _reconciled[index].Save(previous);

            if (!TryFindDivergence(tick - _config.MaxReplayTicks, previous, out var from))
                return;

            _replaying.Value = true;
            try
            {
                Replay(from, previous);
            }
            finally
            {
                _replaying.Value = false;
            }
        }

        private void Replay(int from, int to)
        {
            for (var index = 0; index < _reconciled.Count; index++)
                _reconciled[index].Restore(from);

            for (var replayed = from + 1; replayed <= to; replayed++)
            {
                Step(replayed);
                for (var index = 0; index < _reconciled.Count; index++)
                    _reconciled[index].Save(replayed);
            }
        }

        private void Step(int tick)
        {
            _phases.Fire(TickPhase.Early, tick);
            _phases.Fire(TickPhase.Physics, tick);
            _phases.Fire(TickPhase.Default, tick);
            _phases.Fire(TickPhase.Late, tick);
        }

        private bool TryFindDivergence(int earliest, int latest, out int tick)
        {
            tick = int.MaxValue;
            for (var index = 0; index < _reconciled.Count; index++)
                tick = Earliest(_reconciled[index], earliest, latest, tick);

            for (var index = 0; index < _sources.Count; index++)
                tick = Earliest(_sources[index], earliest, latest, tick);

            return tick != int.MaxValue;
        }

        private static int Earliest(
            IDivergenceSource source,
            int earliest,
            int latest,
            int found) =>
            source.TryGetDivergence(out var diverged) && diverged >= earliest && diverged <= latest && diverged < found ? diverged : found;
    }
}
