using UnityEngine;

namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public interface IMovementCollider
    {
        public float Cast(Vector2 position, Vector2 direction, float distance);
        public float Fall(Vector2 position, float distance);
    }
}
