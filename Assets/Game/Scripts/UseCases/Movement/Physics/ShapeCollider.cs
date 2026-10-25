using System.Collections.Generic;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class ShapeCollider : IMovementCollider
    {
        private const float Skin = 0.015f;
        private const float LaunchProbe = Skin * 2f;

        private readonly PhysicsBody _body;
        private readonly Vector2 _size;
        private readonly Vector2 _offset;
        private readonly PhysicsQuery.QueryFilter _filter;
        private readonly PlatformRider _rider;
        private readonly IShovableCollection _shovables;
        private readonly List<ShovableContact> _ahead = new();

        public ShapeCollider(
            PhysicsBody body,
            BodyBox box,
            LayerMask ground,
            PlatformRider rider,
            IShovableCollection shovables)
        {
            _body = body;
            _size = box.WorldSize();
            _offset = box.WorldOffset();
            _filter = new PhysicsQuery.QueryFilter(PhysicsMask.All, ground.AsPhysicsMask());
            _rider = rider;
            _shovables = shovables;
        }

        public float Cast(Vector2 position, Vector2 direction, float distance)
        {
            var box = BoxAt(position);
            var length = distance + Skin;
            var translation = direction * length;
            using var hits = Hits(box, translation);
            using var inside = Inside(position);

            for (var index = 0; index < hits.Length; index++)
            {
                var hit = hits[index];
                if (!Own(hit) && Vector2.Dot(NormalOf(hit, box, translation), direction) < 0f && !IsPlatform(hit.shape) && !Passable(hit, inside))
                    distance = Mathf.Min(distance, hit.fraction * length - Skin);
            }

            return Mathf.Max(0f, distance);
        }

        public IReadOnlyList<ShovableContact> ShovablesAhead(Vector2 position, Vector2 direction, float distance)
        {
            _ahead.Clear();
            var box = BoxAt(position);
            var length = distance + Skin;
            var translation = direction * length;
            using var hits = Hits(box, translation);

            for (var index = 0; index < hits.Length; index++)
            {
                var hit = hits[index];
                if (Opposing(hit, box, translation) && _shovables.TryGetValue(hit.shape.body, out var shovable))
                    _ahead.Add(new ShovableContact(shovable, hit.fraction * length - Skin));
            }

            return _ahead;
        }

        public float Fall(Vector2 position, float distance)
        {
            var feet = Feet(position);
            var box = BoxAt(position);
            var length = distance + Skin;
            var translation = Vector2.down * length;
            using var hits = Hits(box, translation);
            using var inside = Inside(position);

            for (var index = 0; index < hits.Length; index++)
            {
                var hit = hits[index];
                if (!Own(hit) && NormalOf(hit, box, translation).y > 0f && (!IsPlatform(hit.shape) || BoundsOf(hit.shape).upperBound.y <= feet + Skin) && !Passable(hit, inside))
                    distance = Mathf.Min(distance, hit.fraction * length - Skin);
            }

            return Mathf.Max(0f, distance);
        }

        public Vector2 Carry(Vector2 position, bool grounded, bool dropping)
        {
            if (grounded && _rider.TryGetRidden(position, out var shift))
                position = Shifted(position, shift);

            var lift = _rider.Scooped(position, Skin, dropping).y - position.y;
            return lift > 0f ? position + Vector2.up * Cast(position, Vector2.up, lift) : position;
        }

        public bool TryGetLaunch(Vector2 position, out float height)
        {
            var box = BoxAt(position);
            var translation = Vector2.down * LaunchProbe;
            using var hits = Hits(box, translation);

            for (var index = 0; index < hits.Length; index++)
            {
                var hit = hits[index];
                if (!Own(hit) && NormalOf(hit, box, translation).y > 0f && hit.shape.TryGetComponent<ISpringboard>(out var springboard))
                {
                    height = springboard.LaunchHeight;
                    return true;
                }
            }

            height = 0f;
            return false;
        }

        public bool Is(PhysicsBody body) =>
            body.Equals(_body);

        private Vector2 Shifted(Vector2 position, Vector2 shift)
        {
            if (shift.y != 0f)
            {
                var vertical = shift.y > 0f ? Vector2.up : Vector2.down;
                position += vertical * Cast(position, vertical, Mathf.Abs(shift.y));
            }

            if (shift.x != 0f)
            {
                var horizontal = shift.x > 0f ? Vector2.right : Vector2.left;
                position += horizontal * Cast(position, horizontal, Mathf.Abs(shift.x));
            }

            return position;
        }

        private PolygonGeometry BoxAt(Vector2 position) =>
            PolygonGeometry.CreateBox(_size, 0f, new PhysicsTransform(position + _offset), false);

        private NativeArray<PhysicsQuery.WorldCastResult> Hits(PolygonGeometry box, Vector2 translation) =>
            _body.world.CastGeometry(box, translation, _filter, PhysicsQuery.WorldCastMode.All, Allocator.Temp);

        private NativeArray<PhysicsQuery.WorldOverlapResult> Inside(Vector2 position) =>
            _body.world.OverlapGeometry(PolygonGeometry.CreateBox(_size - Vector2.one * (2f * Skin), 0f, new PhysicsTransform(position + _offset), false), _filter, Allocator.Temp);

        private float Feet(Vector2 position) =>
            position.y + _offset.y - _size.y / 2f;

        private bool Opposing(PhysicsQuery.WorldCastResult hit, PolygonGeometry box, Vector2 translation) =>
            !Own(hit) && Vector2.Dot(NormalOf(hit, box, translation), translation) < 0f && !IsPlatform(hit.shape);

        private bool Own(PhysicsQuery.WorldCastResult hit) =>
            hit.shape.body.Equals(_body);

        private static bool Passable(PhysicsQuery.WorldCastResult hit, NativeArray<PhysicsQuery.WorldOverlapResult> inside)
        {
            if (hit.shape.body.type == PhysicsBody.BodyType.Static)
                return false;

            for (var index = 0; index < inside.Length; index++)
                if (inside[index].shape.Equals(hit.shape))
                    return true;

            return false;
        }

        private static Vector2 NormalOf(PhysicsQuery.WorldCastResult hit, PolygonGeometry box, Vector2 translation) =>
            box.Transform(new PhysicsTransform(translation * hit.fraction)).NormalFrom(hit.shape.polygonGeometry.Transform(hit.shape.body.transform));

        private static PhysicsAABB BoundsOf(PhysicsShape shape) =>
            shape.polygonGeometry.CalculateAABB(shape.body.transform);

        private static bool IsPlatform(PhysicsShape shape) =>
            shape.contactFilter.categories.IsBitSet(PhysicsCategories.OneWay);
    }
}
