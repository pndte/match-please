using Unity.Collections;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics.Extensions
{
    public static class PhysicsWorldExtensions
    {
        public static bool IsFree(this PhysicsWorld world, PolygonGeometry geometry, PhysicsQuery.QueryFilter filter)
        {
            using var found = world.OverlapGeometry(geometry, filter, Allocator.Temp);
            return found.Length == 0;
        }

        public static bool IsFreeOfSolids(
            this PhysicsWorld world,
            PolygonGeometry geometry,
            PhysicsQuery.QueryFilter filter,
            PhysicsBody ignored)
        {
            using var found = world.OverlapGeometry(geometry, filter, Allocator.Temp);
            for (var index = 0; index < found.Length; index++)
                if (!found[index].shape.body.Equals(ignored) && !found[index].shape.IsOneWay())
                    return false;

            return true;
        }
    }
}
