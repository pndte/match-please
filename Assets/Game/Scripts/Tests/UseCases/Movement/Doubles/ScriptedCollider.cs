using System.Collections.Generic;
using Bw.UseCases.Movement.Physics;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Movement.Doubles
{
    public sealed class ScriptedCollider : IMovementCollider
    {
        public float Left { get; set; } = float.NegativeInfinity;
        public float Right { get; set; } = float.PositiveInfinity;
        public float Ceiling { get; set; } = float.PositiveInfinity;
        public float Floor { get; set; } = float.NegativeInfinity;
        public float Plank { get; set; } = float.NegativeInfinity;
        public float LedgeX { get; set; } = float.PositiveInfinity;
        public float LaunchHeight { get; set; }
        public Vector2 CarryShift { get; set; }
        public List<ShovableContact> Ahead { get; } = new();
        public List<string> Calls { get; } = new();

        public float Cast(Vector2 position, Vector2 direction, float distance)
        {
            Calls.Add("Cast " + Named(direction));
            return Mathf.Min(distance, Mathf.Max(0f, Room(position, direction)));
        }

        public IReadOnlyList<ShovableContact> ShovablesAhead(Vector2 position, Vector2 direction, float distance)
        {
            Calls.Add("ShovablesAhead " + Named(direction));
            return Ahead;
        }

        public float Fall(Vector2 position, float distance)
        {
            Calls.Add("Fall");
            var support = Mathf.Max(SolidUnder(position.x), position.x < LedgeX ? Plank : float.NegativeInfinity);
            return Mathf.Min(distance, Mathf.Max(0f, position.y - support));
        }

        public Vector2 Carry(Vector2 position, bool grounded, bool dropping)
        {
            Calls.Add($"Carry {grounded} {dropping}");
            return position + CarryShift;
        }

        public bool TryGetLaunch(Vector2 position, out float height)
        {
            height = LaunchHeight;
            return LaunchHeight > 0f;
        }

        public bool Is(PhysicsBody body) =>
            false;

        private float Room(Vector2 position, Vector2 direction)
        {
            if (direction.x > 0f)
                return Right - position.x;

            if (direction.x < 0f)
                return position.x - Left;

            return direction.y > 0f ? Ceiling - position.y : position.y - SolidUnder(position.x);
        }

        private static string Named(Vector2 direction)
        {
            if (direction.x != 0f)
                return direction.x > 0f ? "right" : "left";

            return direction.y > 0f ? "up" : "down";
        }

        private float SolidUnder(float x) =>
            x < LedgeX ? Floor : float.NegativeInfinity;
    }
}
