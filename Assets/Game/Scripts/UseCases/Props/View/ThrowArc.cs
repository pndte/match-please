using System;
using UnityEngine;

namespace Bw.UseCases.Props.View
{
    public readonly struct ThrowArc : IEquatable<ThrowArc>
    {
        public readonly Vector2 Origin;
        public readonly Vector2 Velocity;
        public readonly Vector2 Gravity;
        public readonly float Duration;

        public ThrowArc(
            Vector2 origin,
            Vector2 velocity,
            Vector2 gravity,
            float duration)
        {
            Origin = origin;
            Velocity = velocity;
            Gravity = gravity;
            Duration = duration;
        }

        public bool Equals(ThrowArc other) =>
            Origin == other.Origin
            && Velocity == other.Velocity
            && Gravity == other.Gravity
            && Duration.Equals(other.Duration);
    }
}
