using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Shooting.Weapon
{
    public readonly struct ShotTrace
    {
        public readonly Vector2 From;
        public readonly Vector2 To;
        public readonly Vector2 Normal;
        public readonly PhysicsShape Shape;
        public readonly bool IsStruck;

        private ShotTrace(
            Vector2 from,
            Vector2 to,
            Vector2 normal,
            PhysicsShape shape,
            bool isStruck)
        {
            From = from;
            To = to;
            Normal = normal;
            Shape = shape;
            IsStruck = isStruck;
        }

        public static ShotTrace Struck(Vector2 from, PhysicsQuery.WorldCastResult hit, Vector2 direction) =>
            new(from, hit.point, hit.normal == Vector2.zero ? -direction : hit.normal, hit.shape, true);

        public static ShotTrace Missed(Vector2 from, Vector2 to) =>
            new(from, to, Vector2.zero, default, false);
    }
}
