using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public sealed class MovementStateView : IStateView<MovementState>
    {
        private readonly IMovementBody _body;

        public MovementStateView(IMovementBody body)
        {
            _body = body;
        }

        public void Show(MovementState from, MovementState to, float progress) =>
            _body.Place(Vector2.Lerp(from.Position, to.Position, progress));
    }
}
