using System;
using System.Collections.Generic;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Physics;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PhysicsRollback : IPhysicsRollback
    {
        private const int MaxReplay = 60;

        private readonly List<IRollbackBody> _bodies = new();
        private readonly PhysicsSpace _space;
        private readonly ISweptBodyCollection _sweeps;
        private readonly IScheduledCollection _scheduled;
        private readonly IPredictableCollection _predictables;

        private int _from = int.MaxValue;

        public PhysicsRollback(
            Lifetime lifetime,
            PhysicsSpace space,
            ISweptBodyCollection sweeps,
            IScheduledCollection scheduled,
            IPredictableCollection predictables,
            INetworkTicks ticks)
        {
            _space = space;
            _sweeps = sweeps;
            _scheduled = scheduled;
            _predictables = predictables;

            ticks.Ticked(TickPhase.Passive).Advise(lifetime, Step);
        }

        public void Track(Lifetime lifetime, IRollbackBody body)
        {
            if (_bodies.Contains(body))
                throw new InvalidOperationException("A body is rolled back once: it is already tracked.");

            _bodies.Add(body);
            lifetime.OnTermination(() => _bodies.Remove(body));
        }

        public void Request(int tick) =>
            _from = Math.Min(_from, tick);

        private void Step(int tick)
        {
            if (_from < tick)
                Replay(Math.Max(_from, tick - MaxReplay), tick - 1);

            _from = int.MaxValue;
            Advance(tick);
            for (var index = _bodies.Count - 1; index >= 0; index--)
                _bodies[index].Verify(tick);
        }

        private void Replay(int from, int to)
        {
            _scheduled.PoseAt(from);
            for (var index = 0; index < _sweeps.Count; index++)
                _sweeps[index].Settle();

            for (var index = 0; index < _bodies.Count; index++)
                _bodies[index].Restore(from);

            for (var tick = from + 1; tick <= to; tick++)
            {
                _scheduled.PoseAt(tick);
                Advance(tick);
            }

            _scheduled.PoseAt(to + 1);
        }

        private void Advance(int tick)
        {
            for (var index = 0; index < _bodies.Count; index++)
                _bodies[index].Prepare(tick);

            _space.Step();
            for (var index = 0; index < _bodies.Count; index++)
                _bodies[index].Finish(tick);

            for (var index = 0; index < _bodies.Count; index++)
                _bodies[index].PredictTouched(tick, _predictables);
        }
    }
}
