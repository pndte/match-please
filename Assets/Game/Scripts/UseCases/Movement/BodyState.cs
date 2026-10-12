using System;
using Bw.UseCases.Movement.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public readonly struct BodyState : IBodyState<BodyState>, IEquatable<BodyState>
    {
        public readonly Vector2 Position;
        public readonly Vector2 Velocity;
        public readonly float HorizontalPush;
        public readonly bool Grounded;

        BodyState IBodyState<BodyState>.Body => this;
        BodyState IBodyState<BodyState>.WithBody(BodyState body) => body;

        public BodyState(Vector2 position, Vector2 velocity, float horizontalPush, bool grounded)
        {
            Position = position;
            Velocity = velocity;
            HorizontalPush = horizontalPush;
            Grounded = grounded;
        }

        public bool Equals(BodyState other) =>
            Position.Equals(other.Position)
            && Velocity.Equals(other.Velocity)
            && HorizontalPush.Equals(other.HorizontalPush)
            && Grounded == other.Grounded;
    }
}
