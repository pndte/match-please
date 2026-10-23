using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Combat.Hands
{
    public sealed class HoldingBody : IMovementBody
    {
        private readonly IMovementBody _body;
        private readonly HandsHold _hold;

        public Vector2 Position => _body.Position;
        public float Angle => _body.Angle;

        public HoldingBody(IMovementBody body, HandsHold hold)
        {
            _body = body;
            _hold = hold;
        }

        public void Place(Vector2 position)
        {
            _body.Place(position);
            _hold.Carry();
        }
    }
}
