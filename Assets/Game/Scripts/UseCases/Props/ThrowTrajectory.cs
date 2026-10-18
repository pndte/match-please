using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Movement;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Props
{
    public sealed class ThrowTrajectory
    {
        private const int Segments = 6;

        public Vector2 Gravity { get; }

        private readonly PhysicsWorld _world;
        private readonly PhysicsQuery.QueryFilter _walls;
        private readonly Vector2 _offset;

        public ThrowTrajectory(
            IPhysicsSpace space,
            BodyBox shape,
            HeldPropConfig held,
            DynamicBodyConfig body)
        {
            _world = space.World;
            _walls = new PhysicsQuery.QueryFilter(PhysicsMask.All, held.Walls.AsPhysicsMask());
            _offset = shape.Offset;
            Gravity = Physics2D.gravity * body.GravityScale;
        }

        public Vector2 CenterOf(RigidState launched) =>
            launched.Position + (Vector2)(Quaternion.Euler(0f, 0f, launched.Angle) * _offset);

        public float ClearTime(RigidState launched, float horizon)
        {
            var origin = CenterOf(launched);
            var step = horizon / Segments;
            var from = origin;
            for (var index = 1; index <= Segments; index++)
            {
                var time = step * index;
                var to = origin + launched.Velocity * time + Gravity * (0.5f * time * time);
                using var hits = _world.CastRay(new PhysicsQuery.CastRayInput(from, to - from), _walls, PhysicsQuery.WorldCastMode.AllSorted, Allocator.Temp);
                foreach (var hit in hits)
                    if (hit.normal != Vector2.zero && !hit.shape.IsOneWay())
                        return step * (index - 1 + hit.fraction);

                from = to;
            }

            return horizon;
        }
    }
}
