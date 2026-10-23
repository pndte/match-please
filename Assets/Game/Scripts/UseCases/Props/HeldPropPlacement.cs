using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Props.Extensions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Props
{
    public sealed class HeldPropPlacement
    {
        private const float Alike = 0.94f;

        private readonly BodyBox _shape;
        private readonly HeldPropConfig _config;
        private readonly PhysicsWorld _world;
        private readonly PhysicsQuery.QueryFilter _walls;

        public HeldPropPlacement(BodyBox shape, HeldPropConfig config, IPhysicsSpace space)
        {
            _shape = shape;
            _config = config;
            _world = space.World;
            _walls = new PhysicsQuery.QueryFilter(PhysicsMask.All, config.Walls.AsPhysicsMask());
        }

        public GripPose PoseIn(Rect oval, float aim, Vector2 pushed) //TODO: в тесноте, где ящику некуда отодвинуться от стены, он наезжает на самого держателя
        {
            var size = _shape.WorldSize();
            var rotation = new PhysicsRotate(aim * Mathf.Deg2Rad);
            var target = oval.center + _config.HeldOffset(aim, oval.size / 2f, size / 2f);
            var placed = PushedOut(oval.center, target, size, rotation, pushed);
            if (TryGetCrossing(oval.center, placed.center, out var crossing))
                placed = PushedOut(oval.center, target - crossing.normal * Vector2.Dot(target - crossing.point, crossing.normal), size, rotation, pushed);

            return new GripPose(placed.center, aim, placed.pushed);
        }

        private (Vector2 center, Vector2 pushed) PushedOut(
            Vector2 from,
            Vector2 target,
            Vector2 size,
            PhysicsRotate rotation,
            Vector2 previous)
        {
            var center = target;
            var pushed = Vector2.zero;
            for (var push = 0; push < _config.Pushes; push++)
            {
                var box = PolygonGeometry.CreateBox(size, 0f, new PhysicsTransform(center, rotation), false);
                if (!TryGetWall(box, out var wall))
                    break;

                var shift = OutOf(box, wall, (from - center).normalized, push == 0 ? previous : Vector2.zero);
                if (push == 0)
                    pushed = shift.normalized;

                center += shift;
            }

            return (center, pushed);
        }

        private bool TryGetCrossing(Vector2 from, Vector2 to, out PhysicsQuery.WorldCastResult crossing)
        {
            using var hits = _world.CastRay(new PhysicsQuery.CastRayInput(from, to - from), _walls, PhysicsQuery.WorldCastMode.AllSorted, Allocator.Temp);
            foreach (var hit in hits)
                if (hit.normal != Vector2.zero && !hit.shape.IsOneWay())
                {
                    crossing = hit;
                    return true;
                }

            crossing = default;
            return false;
        }

        private bool TryGetWall(PolygonGeometry box, out PolygonGeometry wall)
        {
            using var found = _world.OverlapGeometry(box, _walls, Allocator.Temp);
            foreach (var overlap in found)
                if (!overlap.shape.IsOneWay())
                {
                    wall = overlap.shape.polygonGeometry.Transform(overlap.shape.body.transform);
                    return true;
                }

            wall = default;
            return false;
        }

        private Vector2 OutOf(
            PolygonGeometry box,
            PolygonGeometry wall,
            Vector2 toHolder,
            Vector2 preferred)
        {
            var push = Vector2.zero;
            var least = float.PositiveInfinity;
            var kept = Vector2.zero;
            var keptDepth = float.PositiveInfinity;
            var keptAlignment = Alike;
            for (var polygon = 0; polygon < 2; polygon++)
            {
                var axes = polygon == 0 ? box : wall;
                for (var face = 0; face < axes.count; face++)
                {
                    var direction = axes.normals[face];
                    var depth = box.SeparationAlong(wall, direction);
                    var facing = Vector2.Dot(direction, toHolder);
                    var alignment = Vector2.Dot(direction, preferred);
                    if (alignment > keptAlignment && (facing >= -_config.Slack || depth <= _config.Nudge))
                    {
                        keptAlignment = alignment;
                        keptDepth = depth;
                        kept = direction * (depth + PhysicsSlop.Linear);
                    }

                    if (facing < 0f && depth > _config.Nudge || depth >= least)
                        continue;

                    least = depth;
                    push = direction * (depth + PhysicsSlop.Linear);
                }
            }

            return keptDepth <= least + _config.Stickiness ? kept : push;
        }
    }
}
