using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PushedStateView : IStateView<MovementState>
    {
        private readonly IStateView<MovementState> _view;
        private readonly IPredictedPush _push;
        private readonly IMovementCollider _collider;

        public PushedStateView(IStateView<MovementState> view, IPredictedPush push, IMovementCollider collider)
        {
            _view = view;
            _push = push;
            _collider = collider;
        }

        public void Show(MovementState from, MovementState to, float progress)
        {
            var offset = _push.Offset;
            _view.Show(from.Shifted(Reachable(from.Position, offset)), to.Shifted(Reachable(to.Position, offset)), progress);
        }

        private Vector2 Reachable(Vector2 position, Vector2 offset)
        {
            var distance = offset.magnitude;
            if (distance == 0f)
                return offset;

            var direction = offset / distance;
            return direction * _collider.Cast(position, direction, distance);
        }
    }
}
