using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Combat.Hands
{
    public sealed class HoldingBody : IBodyPose
    {
        private readonly IBodyPose _body;
        private readonly HandsHold _hold;

        public HoldingBody(IBodyPose body, HandsHold hold)
        {
            _body = body;
            _hold = hold;
        }

        public void Place(Vector2 position, float angle)
        {
            _body.Place(position, angle);
            _hold.Carry();
        }
    }
}
