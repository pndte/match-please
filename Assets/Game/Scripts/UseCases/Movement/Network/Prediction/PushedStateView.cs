using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PushedStateView<TState> : IStateView<TState> where TState : struct, IBodyState<TState>
    {
        private readonly IStateView<TState> _view;
        private readonly IPredictedPush _push;
        private readonly IMovementCollider _collider;

        public PushedStateView(IStateView<TState> view, IPredictedPush push, IMovementCollider collider)
        {
            _view = view;
            _push = push;
            _collider = collider;
        }

        public void Show(TState from, TState to, float progress)
        {
            var offset = _push.Offset;
            _view.Show(from.Shifted(Reachable(from.Body.Position, offset)), to.Shifted(Reachable(to.Body.Position, offset)), progress);
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
