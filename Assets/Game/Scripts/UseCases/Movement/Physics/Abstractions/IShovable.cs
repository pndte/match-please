using UnityEngine;

namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public interface IShovable
    {
        public float PossibleShove(Vector2 direction, float distance, IMovementCollider shover);
        public void Shove(Vector2 direction, float distance, IMovementCollider shover);
    }
}
