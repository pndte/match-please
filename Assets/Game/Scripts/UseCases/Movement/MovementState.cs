using System;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public readonly struct MovementState : IEquatable<MovementState>
    {
        public readonly Vector2 Position;
        public readonly Vector2 Velocity;
        public readonly float HorizontalPush;
        public readonly bool Grounded;
        public readonly bool RisingFromJump;
        public readonly int CoyoteTicks;
        public readonly int BufferedJumpTicks;

        public MovementState(
            Vector2 position,
            Vector2 velocity,
            float horizontalPush,
            bool grounded,
            bool risingFromJump,
            int coyoteTicks,
            int bufferedJumpTicks)
        {
            Position = position;
            Velocity = velocity;
            HorizontalPush = horizontalPush;
            Grounded = grounded;
            RisingFromJump = risingFromJump;
            CoyoteTicks = coyoteTicks;
            BufferedJumpTicks = bufferedJumpTicks;
        }

        public bool Equals(MovementState other) =>
            Position.Equals(other.Position)
            && Velocity.Equals(other.Velocity)
            && HorizontalPush.Equals(other.HorizontalPush)
            && Grounded == other.Grounded
            && RisingFromJump == other.RisingFromJump
            && CoyoteTicks == other.CoyoteTicks
            && BufferedJumpTicks == other.BufferedJumpTicks;
    }
}
