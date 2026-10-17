using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Props.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Props
{
    //TODO: плохо, декомпозировать
    public sealed class PropStateView : IStateView<PropState>, IHeldItem
    {
        private const int Pushes = 4;
        private const float Skin = 0.005f;
        private const float Stickiness = 0.3f;
        private const float Alike = 0.94f;
        private const float Slack = 0.3f;
        private const float Nudge = 0.15f;

        public ISource<Unit> Turned => _turned;

        private readonly Transform _prop;
        private readonly BodyBox _shape;
        private readonly HeldPropConfig _config;
        private readonly HeldPropGrip _grip;
        private readonly Signal<Unit> _turned = new();

        private readonly PhysicsWorld _world;
        private readonly PhysicsQuery.QueryFilter _walls;

        private float _aim;
        private Vector2 _pushed;

        public PropStateView(
            BodyBox shape,
            IReadonlySimulation<PropState> simulation,
            HeldPropConfig config,
            HeldPropGrip grip,
            IPhysicsSpace space)
        {
            _prop = shape.transform;
            _shape = shape;
            _config = config;
            _grip = grip;
            _world = space.World;
            _walls = new PhysicsQuery.QueryFilter(PhysicsMask.All, config.Walls.AsPhysicsMask());
            _aim = simulation.State.Value.Aim;
        }

        public void Show(PropState from, PropState to, float progress)
        {
            _aim = Mathf.LerpAngle(from.Aim, to.Aim, progress);
            _turned.Fire(Unit.Instance);
        }

        public void PlaceIn(Rect oval) //TODO: в тесноте, где ящику некуда отодвинуться от стены, он наезжает на самого держателя; рук у держателя нет, ящик висит перед ним, как парящее оружие
        {
            var size = _shape.WorldSize();
            var turn = Quaternion.Euler(0f, 0f, _aim);
            var placed = Placed(oval.center, oval.center + _config.HeldOffset(_aim, oval.size / 2f, size / 2f), size);
            _pushed = placed.pushed;
            var position = placed.center - (Vector2)(turn * _shape.WorldOffset());
            _prop.SetPositionAndRotation(new Vector3(position.x, position.y, _prop.position.z), turn);
            _grip.Place(placed.center, _aim);
        }

        private (Vector2 center, Vector2 pushed) Placed(Vector2 from, Vector2 target, Vector2 size)
        {
            var placed = PushedOut(from, target, size);
            return TryGetCrossing(from, placed.center, out var crossing)
                ? PushedOut(from, target - crossing.normal * Vector2.Dot(target - crossing.point, crossing.normal), size)
                : placed;
        }

        private (Vector2 center, Vector2 pushed) PushedOut(Vector2 from, Vector2 target, Vector2 size)
        {
            var rotation = new PhysicsRotate(_aim * Mathf.Deg2Rad);
            var center = target;
            var pushed = Vector2.zero;
            for (var push = 0; push < Pushes; push++)
            {
                var box = PolygonGeometry.CreateBox(size, 0f, new PhysicsTransform(center, rotation), false);
                if (!TryGetWall(box, out var wall))
                    break;

                var shift = OutOf(box, wall, (from - center).normalized, push == 0 ? _pushed : Vector2.zero);
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

        private static Vector2 OutOf(
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
                    var depth = Depth(box, wall, direction);
                    var facing = Vector2.Dot(direction, toHolder);
                    var alignment = Vector2.Dot(direction, preferred);
                    if (alignment > keptAlignment && (facing >= -Slack || depth <= Nudge))
                    {
                        keptAlignment = alignment;
                        keptDepth = depth;
                        kept = direction * (depth + Skin);
                    }

                    if (facing < 0f && depth > Nudge || depth >= least)
                        continue;

                    least = depth;
                    push = direction * (depth + Skin);
                }
            }

            return keptDepth <= least + Stickiness ? kept : push;
        }

        private static float Depth(PolygonGeometry box, PolygonGeometry wall, Vector2 normal)
        {
            var deepest = float.NegativeInfinity;
            for (var vertex = 0; vertex < wall.count; vertex++)
                deepest = Mathf.Max(deepest, Vector2.Dot(normal, wall.vertices[vertex]));

            var nearest = float.PositiveInfinity;
            for (var vertex = 0; vertex < box.count; vertex++)
                nearest = Mathf.Min(nearest, Vector2.Dot(normal, box.vertices[vertex]));

            return Mathf.Max(0f, deepest - nearest);
        }
    }
}
