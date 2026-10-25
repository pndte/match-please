using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement
{
    public sealed class BodyPoseView<TState> : IStateView<TState> where TState : struct, IPlacedState<TState>
    {
        private readonly PhysicsBody _body;
        private readonly IMovementBody _picture;

        public BodyPoseView(PhysicsBody body, IMovementBody picture)
        {
            _body = body;
            _picture = picture;
        }

        public void Show(TState from, TState to, float progress)
        {
            var position = Vector2.Lerp(from.Position, to.Position, progress);
            _body.position = position;
            _picture.Place(position);
        }
    }
}
