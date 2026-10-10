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
        public readonly JumpState Jump;

        public MovementState(Vector2 position, Vector2 velocity, float horizontalPush, bool grounded, JumpState jump)
        {
            Position = position;
            Velocity = velocity;
            HorizontalPush = horizontalPush;
            Grounded = grounded;
            Jump = jump;
        }

        public bool Equals(MovementState other) =>
            Position.Equals(other.Position)
            && Velocity.Equals(other.Velocity)
            && HorizontalPush.Equals(other.HorizontalPush)
            && Grounded == other.Grounded
            && Jump.Equals(other.Jump);
    }
}
