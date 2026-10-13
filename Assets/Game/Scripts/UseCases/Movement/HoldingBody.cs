using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public sealed class HoldingBody : IMovementBody
    {
        private readonly IMovementBody _body;
        private readonly IHeldItems _held;

        public Vector2 Position => _body.Position;

        public HoldingBody(IMovementBody body, IHeldItems held)
        {
            _body = body;
            _held = held;
        }

        public void Place(Vector2 position)
        {
            _body.Place(position);
            _held.Carry();
        }
    }
}
