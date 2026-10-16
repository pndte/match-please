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

            var pivot = shape.polygonGeometry.centroid - box.WorldOffset();
            return new Contact(shape.body.transform.InverseTransformPoint(point) - pivot);
        }
    }
}
