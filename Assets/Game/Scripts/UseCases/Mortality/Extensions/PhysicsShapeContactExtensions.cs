using System;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Mortality.Extensions
{
    public static class PhysicsShapeContactExtensions
    {
        public static Contact ContactAt(this PhysicsShape shape, Vector2 point)
        {
            if (shape.shapeType != PhysicsShape.ShapeType.Polygon || !shape.TryGetComponent<BodyBox>(out var box))
                throw new InvalidOperationException("A contact is taken on the box of an object with a BodyBox, and the struck shape is not such a box.");

            var polygon = shape.polygonGeometry;
            var turn = polygon.normals[1];
            var pivot = polygon.centroid - Turned(box.WorldOffset(), turn);
            return new Contact(Unturned(shape.body.transform.InverseTransformPoint(point) - pivot, turn));
        }

        private static Vector2 Turned(Vector2 vector, Vector2 turn) =>
            new(turn.x * vector.x - turn.y * vector.y, turn.y * vector.x + turn.x * vector.y);

        private static Vector2 Unturned(Vector2 vector, Vector2 turn) =>
            new(turn.x * vector.x + turn.y * vector.y, turn.x * vector.y - turn.y * vector.x);
    }
}
