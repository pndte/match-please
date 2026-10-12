using System.Collections.Generic;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class BoxBody : IMovementBody, IMovementCollider
    {
        private const float Skin = 0.015f;
        private const float RideTolerance = 0.05f;
        private const float LaunchProbe = Skin * 2f;

        public Vector2 Position => _transform.position;

        private readonly Rigidbody2D _rigidbody;
        private readonly Collider2D _collider;
        private readonly Transform _transform;
        private readonly Vector2 _size;
        private readonly Vector2 _offset;
        private readonly ContactFilter2D _filter;
        private readonly IMovingPlatformCollection _platforms;
        private readonly List<RaycastHit2D> _hits = new(4);

        public BoxBody(Rigidbody2D rigidbody, BoxCollider2D collider, IBodyConfig config, IMovingPlatformCollection platforms)
        {
            _rigidbody = rigidbody;
            _collider = collider;
            _transform = rigidbody.transform;
            _size = Vector2.Scale(collider.size, _transform.lossyScale);
            _offset = Vector2.Scale(collider.offset, _transform.lossyScale);
            _filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = config.GroundLayer,
                useTriggers = false
            };
            _platforms = platforms;
        }

        public void Place(Vector2 position)
        {
            _rigidbody.position = position;
            _transform.position = new Vector3(position.x, position.y, _transform.position.z);
        }

        public float Cast(Vector2 position, Vector2 direction, float distance)
        {
            var hitCount = Physics2D.BoxCast(position + _offset, _size, 0f, direction, _filter, _hits, distance + Skin);

            for (var i = 0; i < hitCount; i++)
            {
                var hit = _hits[i];
                if (hit.collider != _collider && Vector2.Dot(hit.normal, direction) < 0f && !IsPlatform(hit.collider))
                    distance = Mathf.Min(distance, hit.distance - Skin);
            }

            return Mathf.Max(0f, distance);
        }

        public float Fall(Vector2 position, float distance)
        {
            var feet = Feet(position);
            var hitCount = Physics2D.BoxCast(position + _offset, _size, 0f, Vector2.down, _filter, _hits, distance + Skin);

            for (var i = 0; i < hitCount; i++)
            {
                var hit = _hits[i];
                if (hit.collider != _collider && hit.normal.y > 0f && (!IsPlatform(hit.collider) || hit.collider.bounds.max.y <= feet + Skin))
                    distance = Mathf.Min(distance, hit.distance - Skin);
            }

            return Mathf.Max(0f, distance);
        }

        public Vector2 Carry(Vector2 position, bool grounded)
        {
            if (grounded && TryGetRidden(position, out var shift))
                position = Shifted(position, shift);

            return Scooped(position);
        }

        public bool TryGetLaunch(Vector2 position, out float speed)
        {
            var hitCount = Physics2D.BoxCast(position + _offset, _size, 0f, Vector2.down, _filter, _hits, LaunchProbe);

            for (var i = 0; i < hitCount; i++)
            {
                var hit = _hits[i];
                if (hit.normal.y > 0f && hit.collider.TryGetComponent(out ISpringboard springboard))
                {
                    speed = springboard.LaunchSpeed;
                    return true;
                }
            }

            speed = 0f;
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

        private float Feet(Vector2 position) =>
            position.y + _offset.y - _size.y / 2f;

        private bool Riding(Rect area, Vector2 position) =>
            Overlapping(area, position) && Mathf.Abs(Feet(position) - area.yMax) <= RideTolerance;

        private bool Overlapping(Rect area, Vector2 position)
        {
            var left = position.x + _offset.x - _size.x / 2f;
            return left < area.xMax && left + _size.x > area.xMin;
        }

        private static bool IsPlatform(Collider2D collider) => //TODO: мотор понимает только доски, которые ловят сверху: поворот и дуга эффектора (rotationalOffset, surfaceArc) не учитываются
            collider.usedByEffector && collider.TryGetComponent(out PlatformEffector2D effector) && effector.enabled && effector.useOneWay;
    }
}
