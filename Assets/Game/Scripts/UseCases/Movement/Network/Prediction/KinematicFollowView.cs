using Bw.Entities.Network.Rollback;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class KinematicFollowView<TState> : IStateView<TState> where TState : struct, IPlacedState<TState> //TODO: при пересчёте тела вне пузыря стоят там, где их показывал последний кадр, а не на пересчитываемом тике: столкновение предсказанного с непредсказанным в пересчёте приблизительное
    {
        private readonly IStateView<TState> _view;
        private readonly IPredictedBy _predictedBy;
        private readonly IBodyPose _pose;
        private readonly PhysicsBody _body;

        public KinematicFollowView(
            IStateView<TState> view,
            IPredictedBy predictedBy,
            IBodyPose pose,
            PhysicsBody body)
        {
            _view = view;
            _predictedBy = predictedBy;
            _pose = pose;
            _body = body;
        }

        public void Show(TState from, TState to, float progress)
        {
            if (!_predictedBy.Me.Value)
                Place(Vector2.Lerp(from.Position, to.Position, progress), Mathf.LerpAngle(from.Angle, to.Angle, progress));

            _view.Show(from, to, progress);
        }

        private void Place(Vector2 position, float angle)
        {
            _pose.Place(position, angle);
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
        }
    }
}
