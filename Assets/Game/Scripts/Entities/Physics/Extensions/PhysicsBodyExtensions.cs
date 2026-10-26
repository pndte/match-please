using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics.Extensions
{
    public static class PhysicsBodyExtensions
    {
        private const ulong LayerBits = 0xFFFFFFFFul;
        private const float ProbeRadius = 0.01f;

        public static PhysicsShape CreateShape(
            this PhysicsBody body,
            PolygonGeometry geometry,
            PhysicsShapeDefinition definition,
            GameObject owner)
        {
            var shape = body.CreateShape(geometry, definition);
            shape.userData = new PhysicsUserData { objectValue = owner };
            return shape;
        }

        public static void MoveToLayer(this PhysicsBody body, int layer)
        {
            using var shapes = body.GetShapes(Allocator.Temp);
            for (var index = 0; index < shapes.Length; index++)
            {
                var shape = shapes[index];
                var filter = shape.contactFilter;
                var categories = filter.categories;
                if (categories.bitMask == 0)
                    continue;

                categories.bitMask = (categories.bitMask & ~LayerBits) | (1ul << layer);
                filter.categories = categories;
                shape.contactFilter = filter;
            }
        }

        public static void SweepContinuously(this PhysicsBody body, Vector2 center)
        {
            var probe = PhysicsShapeDefinition.defaultDefinition;
            probe.density = 0f;
            probe.contactFilter = new PhysicsShape.ContactFilter(PhysicsMask.None, PhysicsMask.None, 0);
            body.fastCollisionsAllowed = true;
            body.CreateShape(new CircleGeometry { center = center, radius = ProbeRadius }, probe);
        }
    }
}
