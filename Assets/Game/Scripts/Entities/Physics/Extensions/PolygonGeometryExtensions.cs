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
