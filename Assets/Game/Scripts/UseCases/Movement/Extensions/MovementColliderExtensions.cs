using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Extensions
{
    public static class MovementColliderExtensions
    {
        public static Vector2 ReachableOffset(this IMovementCollider collider, Vector2 position, Vector2 offset)
        {
            var horizontal = collider.Along(position, new Vector2(Mathf.Sign(offset.x), 0f), Mathf.Abs(offset.x));
            var moved = position + horizontal;
            var vertical = offset.y < 0f ? Vector2.down * collider.Fall(moved, -offset.y) : collider.Along(moved, Vector2.up, offset.y);
            return horizontal + vertical;
        }

        public static Vector2 ReachableDrift(this Vector2 drift, Vector2 offset, Vector2 reachable) =>
            new(reachable.x == offset.x ? drift.x : 0f, reachable.y == offset.y ? drift.y : 0f);

        private static Vector2 Along(
            this IMovementCollider collider,
            Vector2 position,
            Vector2 direction,
            float distance) =>
            distance > 0f ? direction * collider.Cast(position, direction, distance) : Vector2.zero;
    }
}
