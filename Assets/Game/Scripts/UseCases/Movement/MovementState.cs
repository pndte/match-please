using UnityEngine;

namespace Bw.UseCases.Movement
{
    public readonly struct MovementState
    {
        public readonly Vector2 Position;
        public readonly Vector2 Velocity;
        public readonly bool Grounded;

        public MovementState(Vector2 position, Vector2 velocity, bool grounded)
        {
            Position = position;
            Velocity = velocity;
            Grounded = grounded;
        }
    }
}
