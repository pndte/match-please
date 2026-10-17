using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics.Extensions
{
    public static class PolygonGeometryExtensions
    {
        private const float FaceTolerance = 0.0005f;

        public static Vector2 NormalFrom(this PolygonGeometry polygon, PolygonGeometry other)
        {
            var own = MaxSeparation(polygon, other, out var ownNormal);
            var opposite = MaxSeparation(other, polygon, out var oppositeNormal);
            return opposite > own + FaceTolerance ? oppositeNormal : -ownNormal;
        }

        public static float DistanceTo(this PolygonGeometry polygon, Vector2 point) =>
            polygon.OverlapPoint(point) ? 0f : Vector2.Distance(point, polygon.ClosestPoint(point));

        public static float SeparationAlong(this PolygonGeometry polygon, PolygonGeometry other, Vector2 direction)
        {
            var farthest = float.NegativeInfinity;
            for (var vertex = 0; vertex < other.count; vertex++)
                farthest = Mathf.Max(farthest, Vector2.Dot(direction, other.vertices[vertex]));

            var nearest = float.PositiveInfinity;
            for (var vertex = 0; vertex < polygon.count; vertex++)
                nearest = Mathf.Min(nearest, Vector2.Dot(direction, polygon.vertices[vertex]));

            return Mathf.Max(0f, farthest - nearest);
        }

        private static float MaxSeparation(PolygonGeometry polygon, PolygonGeometry other, out Vector2 normal)
        {
            var separation = float.NegativeInfinity;
            normal = Vector2.zero;
            for (var face = 0; face < polygon.count; face++)
            {
                var faceNormal = polygon.normals[face];
                var faceVertex = polygon.vertices[face];
                var nearest = float.PositiveInfinity;
                for (var vertex = 0; vertex < other.count; vertex++)
                    nearest = Mathf.Min(nearest, Vector2.Dot(faceNormal, other.vertices[vertex] - faceVertex));

                if (nearest <= separation)
                    continue;

                separation = nearest;
                normal = faceNormal;
            }

            return separation;
        }
    }
}
