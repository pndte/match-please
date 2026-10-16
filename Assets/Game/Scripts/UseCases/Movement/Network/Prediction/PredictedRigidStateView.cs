using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PredictedRigidStateView : IStateView<RigidState>
    {
        private readonly IStateView<RigidState> _view;
        private readonly IPredictedRigidBody _body;
        private readonly INetworkTicks _ticks;
        private readonly IInterpolationTicks _interpolation;

        public PredictedRigidStateView(
            IStateView<RigidState> view,
            IPredictedRigidBody body,
            INetworkTicks ticks,
            IInterpolationTicks interpolation)
        {
            _view = view;
            _body = body;
            _ticks = ticks;
            _interpolation = interpolation;
        }

        public void Show(RigidState from, RigidState to, float progress)
        {
            var tick = _ticks.PredictedViewTick(_interpolation);
            if (_body.TryGetShown(tick, out var shown))
            {
                _view.Show(shown, shown, 0f);
                return;
            }

            _body.Follow(tick, new RigidState(Vector2.Lerp(from.Position, to.Position, progress), Mathf.LerpAngle(from.Angle, to.Angle, progress), Vector2.zero, 0f));
            _view.Show(from, to, progress);
        }
    }
}
