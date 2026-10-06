using System;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public readonly struct MovementState : IEquatable<MovementState>
    {
        public readonly Vector2 Position;
        public readonly Vector2 Velocity;
        public readonly bool Grounded;
        public readonly int CoyoteTicks;
        public readonly int BufferedJumpTicks;

        public MovementState(Vector2 position, Vector2 velocity, bool grounded, int coyoteTicks, int bufferedJumpTicks)
        {
            Position = position;
            Velocity = velocity;
            Grounded = grounded;
            CoyoteTicks = coyoteTicks;
            BufferedJumpTicks = bufferedJumpTicks;
        }

        public bool Equals(MovementState other) =>
            Position.Equals(other.Position)
            && Velocity.Equals(other.Velocity)
            && Grounded == other.Grounded
            && CoyoteTicks == other.CoyoteTicks
            && BufferedJumpTicks == other.BufferedJumpTicks;
    }
}
