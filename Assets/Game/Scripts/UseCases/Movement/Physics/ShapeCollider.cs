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
        private const float RideTolerance = 0.05f;
        private const float LaunchProbe = Skin * 2f;

        private readonly PhysicsBody _body;
        private readonly Vector2 _size;
        private readonly Vector2 _offset;
        private readonly PhysicsQuery.QueryFilter _filter;
        private readonly IMovingPlatformCollection _platforms;
        private readonly IShovableCollection _shovables;
        private readonly List<ShovableContact> _ahead = new();

        public ShapeCollider(
            PhysicsBody body,
            BoxCollider2D collider,
            LayerMask ground,
            IMovingPlatformCollection platforms,
            IShovableCollection shovables)
        {
            _body = body;
            _size = collider.WorldSize();
            _offset = collider.WorldOffset();
            _filter = new PhysicsQuery.QueryFilter(PhysicsMask.All, ground.AsPhysicsMask());
            _platforms = platforms;
            _shovables = shovables;
        }

        public float Cast(Vector2 position, Vector2 direction, float distance)
        {
            var box = BoxAt(position);
            var length = distance + Skin;
            var translation = direction * length;
            using var hits = Hits(box, translation);

            for (var index = 0; index < hits.Length; index++)
            {
                var hit = hits[index];
                if (!Own(hit) && Vector2.Dot(NormalOf(hit, box, translation), direction) < 0f && !IsPlatform(hit.shape))
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

            for (var index = 0; index < hits.Length; index++)
            {
                var hit = hits[index];
                if (!Own(hit) && NormalOf(hit, box, translation).y > 0f && (!IsPlatform(hit.shape) || BoundsOf(hit.shape).upperBound.y <= feet + Skin))
                    distance = Mathf.Min(distance, hit.fraction * length - Skin);
            }

            return Mathf.Max(0f, distance);
        }

        public Vector2 Carry(Vector2 position, bool grounded)
        {
            if (grounded && TryGetRidden(position, out var shift))
                position = Shifted(position, shift);

            return Scooped(position);
        }

        public bool TryGetLaunch(Vector2 position, out float height)
        {
            var box = BoxAt(position);
            var translation = Vector2.down * LaunchProbe;
            using var hits = Hits(box, translation);

            for (var index = 0; index < hits.Length; index++)
            {
                var hit = hits[index];
                if (!Own(hit) && NormalOf(hit, box, translation).y > 0f && hit.shape.userData.objectValue is ISpringboard springboard)
                {
                    height = springboard.LaunchHeight;
                    return true;
                }
            }

            height = 0f;
            return false;
        }

        public Vector2 RideShift(Vector2 position, double fromSeconds, double toSeconds)
        {
            var top = float.NegativeInfinity;
            var shift = Vector2.zero;

            for (var index = 0; index < _platforms.Count; index++)
            {
                var platform = _platforms[index];
                var then = platform.AreaAt(fromSeconds);
                if (!Riding(then, position) || then.yMax <= top)
                    continue;

                top = then.yMax;
                shift = platform.AreaAt(toSeconds).position - then.position;
            }

            return shift;
        }

        public bool Is(PhysicsBody body) =>
            body.Equals(_body);

        private bool TryGetRidden(Vector2 position, out Vector2 shift)
        {
            var top = float.NegativeInfinity;
            var found = false;
            shift = Vector2.zero;

            for (var index = 0; index < _platforms.Count; index++)
            {
                var platform = _platforms[index];
                var previous = new Rect(platform.Area.position - platform.Shift, platform.Area.size);
                if (!Riding(previous, position) || previous.yMax <= top)
                    continue;

                top = previous.yMax;
                shift = platform.Shift;
                found = true;
            }

            return found;
        }

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

        private Vector2 Scooped(Vector2 position) //TODO: подхват поднимающейся платформой не знает про S — со сквозной доски, которая едет вверх, на медленном ходу не спрыгнуть: она подхватывает снова; потолок над платформой подхват не проверяет, поэтому лифт может вдавить ящик в стоящего над ним персонажа, а влезших друг в друга мотор не расталкивает: оба упираются друг в друга, пока ящик не разобьют
        {
            var feet = Feet(position);
            var lift = 0f;

            for (var index = 0; index < _platforms.Count; index++)
            {
                var platform = _platforms[index];
                var area = platform.Area;
                var previousTop = area.yMax - platform.Shift.y;
                if (platform.Shift.y > 0f && Overlapping(area, position) && feet >= previousTop - RideTolerance && feet < area.yMax + Skin)
                    lift = Mathf.Max(lift, area.yMax + Skin - feet);
            }

            return position + Vector2.up * lift;
        }

        private PolygonGeometry BoxAt(Vector2 position) =>
            PolygonGeometry.CreateBox(_size, 0f, new PhysicsTransform(position + _offset), false);

        private NativeArray<PhysicsQuery.WorldCastResult> Hits(PolygonGeometry box, Vector2 translation) =>
            _body.world.CastGeometry(box, translation, _filter, PhysicsQuery.WorldCastMode.All, Allocator.Temp);

        private float Feet(Vector2 position) =>
            position.y + _offset.y - _size.y / 2f;

        private bool Riding(Rect area, Vector2 position) =>
            Overlapping(area, position) && Mathf.Abs(Feet(position) - area.yMax) <= RideTolerance;

        private bool Overlapping(Rect area, Vector2 position)
        {
            var left = position.x + _offset.x - _size.x / 2f;
            return left < area.xMax && left + _size.x > area.xMin;
        }

        private bool Opposing(PhysicsQuery.WorldCastResult hit, PolygonGeometry box, Vector2 translation) =>
            !Own(hit) && Vector2.Dot(NormalOf(hit, box, translation), translation) < 0f && !IsPlatform(hit.shape);

        private bool Own(PhysicsQuery.WorldCastResult hit) =>
            hit.shape.body.Equals(_body);

        private static Vector2 NormalOf(PhysicsQuery.WorldCastResult hit, PolygonGeometry box, Vector2 translation) =>
            box.Transform(new PhysicsTransform(translation * hit.fraction)).NormalFrom(hit.shape.polygonGeometry.Transform(hit.shape.body.transform));

        private static PhysicsAABB BoundsOf(PhysicsShape shape) =>
            shape.polygonGeometry.CalculateAABB(shape.body.transform);

        private static bool IsPlatform(PhysicsShape shape) => //TODO: мотор понимает только доски, которые ловят сверху: поворот и дуга эффектора (rotationalOffset, surfaceArc) не учитываются
            shape.contactFilter.categories.IsBitSet(PhysicsCategories.OneWay);
    }
}
