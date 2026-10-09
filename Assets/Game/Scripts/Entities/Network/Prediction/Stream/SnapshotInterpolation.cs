using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class SnapshotInterpolation<TState> where TState : struct
    {
        private const int MaxSnapshots = 64;

        private readonly List<TickedState<TState>> _snapshots = new();
        private readonly IInterpolationTicks _ticks;
        private readonly IStateView<TState> _view;

        public SnapshotInterpolation(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IInterpolationTicks ticks,
            INetResultReceiver<TickedState<TState>> state,
            IStateView<TState> view)
        {
            _ticks = ticks;
            _view = view;

            controlledBy.Me.WhenFalse(lifetime, remoteLifetime =>
            {
                state.Received.Advise(remoteLifetime, Remember);
                Observable.EveryUpdate(UnityFrameProvider.Update, remoteLifetime).Subscribe(UpdateView);
                remoteLifetime.OnTermination(_snapshots.Clear);
            });
        }

        private void Remember(TickedState<TState> snapshot)
        {
            if (_snapshots.Count > 0 && snapshot.Tick <= _snapshots[_snapshots.Count - 1].Tick)
                return;

            _snapshots.Add(snapshot);
            if (_snapshots.Count > MaxSnapshots)
                _snapshots.RemoveAt(0);
        }

        private void UpdateView(Unit _)
        {
            if (_snapshots.Count == 0)
                return;

            var renderTick = _ticks.InterpolationTick;
            while (_snapshots.Count > 1 && _snapshots[1].Tick <= renderTick)
                _snapshots.RemoveAt(0);

            var from = _snapshots[0];
            if (_snapshots.Count == 1 || renderTick <= from.Tick)
            {
                _view.Show(from.State, from.State, 0f);
                return;
            }

            var to = _snapshots[1];
            var progress = (float)((renderTick - from.Tick) / (to.Tick - from.Tick));
            _view.Show(from.State, to.State, progress);
        }
    }
}
