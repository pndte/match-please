using System;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class SnapshotInterpolator<TState> where TState : struct
    {
        private readonly IInterpolationTicks _ticks;
        private readonly IReplicatedStates<TState> _server;
        private readonly IStateView<TState> _view;

        public SnapshotInterpolator(
            Lifetime lifetime,
            IPredictedBy predictedBy,
            IInterpolationTicks ticks,
            IReplicatedStates<TState> server,
            IStateView<TState> view)
        {
            _ticks = ticks;
            _server = server;
            _view = view;

            predictedBy.Me.WhenFalse(lifetime, interpolatedLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.Update, interpolatedLifetime).Subscribe(UpdateView));
        }

        private void UpdateView(Unit frame)
        {
            var renderTick = _ticks.InterpolationTick;
            var shownTick = (int)Math.Floor(renderTick);
            if (!_server.TryGetLatest(shownTick, out var fromTick, out var from))
            {
                if (_server.TryGetEarliest(shownTick + 1, out _, out var first))
                    _view.Show(first, first, 0f);

                return;
            }

            if (!_server.TryGetEarliest(fromTick + 1, out var toTick, out var to))
            {
                _view.Show(from, from, 0f);
                return;
            }

            _view.Show(from, to, (float)((renderTick - fromTick) / (toTick - fromTick)));
        }
    }
}
