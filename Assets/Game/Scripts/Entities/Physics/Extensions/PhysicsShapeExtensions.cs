using System;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics.Extensions
{
    public static class PhysicsShapeExtensions
    {
        private const int Layers = 32;

        public static int Layer(this PhysicsShape shape)
        {
            var categories = shape.contactFilter.categories;
            var found = -1;
            for (var layer = 0; layer < Layers; layer++)
            {
                if (!categories.IsBitSet(layer))
                    continue;

                if (found >= 0)
                    throw new InvalidOperationException($"A shape belongs to one layer, but its categories hold layers {found.ToString()} and {layer.ToString()}.");

                found = layer;
            }

            if (found < 0)
                throw new InvalidOperationException("A shape belongs to one layer, but its categories hold none.");

            return found;
        }

        public static bool TryGetComponent<T>(this PhysicsShape shape, out T component)
        {
            if (shape.userData.objectValue is GameObject owner)
                return owner.TryGetComponent(out component);

            component = default;
            return false;
        }

        public static bool IsOneWay(this PhysicsShape shape) =>
            shape.contactFilter.categories.IsBitSet(PhysicsCategories.OneWay);

        public static float DistanceTo(this PhysicsShape shape, Vector2 point) =>
            shape.OverlapPoint(point) ? 0f : Vector2.Distance(point, shape.ClosestPoint(point));
    }
}
