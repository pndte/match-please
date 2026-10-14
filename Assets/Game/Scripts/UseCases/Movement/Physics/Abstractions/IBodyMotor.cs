using UnityEngine;

namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public interface IBodyMotor
    {
        public BodyState Step(BodyState state, BodyDrive drive, out bool launched);
        public float PossibleShove(
            Vector2 position,
            Vector2 direction,
            float distance,
            IMovementCollider shover);
        public void MakeWay(
            Vector2 position,
            Vector2 direction,
            float distance,
            IMovementCollider shover);
    }
}
