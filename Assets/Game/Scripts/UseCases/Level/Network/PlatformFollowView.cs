using Bw.Entities.Network.Rollback;
using Bw.Entities.Simulation;
using Bw.UseCases.Level.Extensions;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Level.Network
{
    public sealed class PlatformFollowView : IStateView<PlatformState>
    {
        private readonly IStateView<PlatformState> _view;
        private readonly IPredictedBy _predictedBy;
        private readonly PhysicsBody _body;
        private readonly PlatformRail _rail;

        public PlatformFollowView(
            IStateView<PlatformState> view,
            IPredictedBy predictedBy,
            PhysicsBody body,
            PlatformRail rail)
        {
            _view = view;
            _predictedBy = predictedBy;
            _body = body;
            _rail = rail;
        }

        public void Show(PlatformState from, PlatformState to, float progress)
        {
            if (!_predictedBy.Me.Value)
                Place(_rail.PositionAt(from, to, progress));

            _view.Show(from, to, progress);
        }

        private void Place(Vector2 position)
        {
            if (_body.position != position)
                _body.position = position;

            _body.linearVelocity = Vector2.zero;
        }
    }
}
