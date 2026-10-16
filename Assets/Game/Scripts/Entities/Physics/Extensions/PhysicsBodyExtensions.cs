using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics.Extensions
{
    public static class PhysicsBodyExtensions
    {
        private const ulong LayerBits = 0xFFFFFFFFul;
        private const float ProbeRadius = 0.01f;

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

        public static void SweepContinuously(this PhysicsBody body, Vector2 center) //TODO: два таких тела друг против друга Box2D не проверяет непрерывно («пуля» против «пули»): быстрый ящик может на шаг войти в другой и вытолкнуться
        {
            var probe = PhysicsShapeDefinition.defaultDefinition;
            probe.density = 0f;
            probe.contactFilter = new PhysicsShape.ContactFilter(PhysicsMask.None, PhysicsMask.None, 0);
            body.fastCollisionsAllowed = true;
            body.CreateShape(new CircleGeometry { center = center, radius = ProbeRadius }, probe);
        }
    }
}
