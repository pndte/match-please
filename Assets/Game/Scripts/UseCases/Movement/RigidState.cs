using System;
using Bw.UseCases.Movement.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public readonly struct RigidState : IPlacedState<RigidState>, IEquatable<RigidState>
    {
        public readonly Vector2 Position;
        public readonly float Angle;
        public readonly Vector2 Velocity;
        public readonly float AngularVelocity;

        Vector2 IPlacedState<RigidState>.Position => Position;

        public RigidState(
            Vector2 position,
            float angle,
            Vector2 velocity,
            float angularVelocity)
        {
            Position = position;
            Angle = angle;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
        }

        RigidState IPlacedState<RigidState>.Shifted(Vector2 offset) =>
            new(Position + offset, Angle, Velocity, AngularVelocity);

        public bool Equals(RigidState other) =>
            Position.Equals(other.Position)
            && Angle.Equals(other.Angle)
            && Velocity.Equals(other.Velocity)
            && AngularVelocity.Equals(other.AngularVelocity);
    }
}
