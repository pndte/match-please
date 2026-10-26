using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class PlatformRider
    {
        private const float RideTolerance = 0.05f;

        private readonly Vector2 _size;
        private readonly Vector2 _offset;
        private readonly IMovingPlatformCollection _platforms;
        private readonly IShovableCollection _shovables;
        private readonly ISimulationStep _step;

        public PlatformRider(
            BodyBox box,
            IMovingPlatformCollection platforms,
            IShovableCollection shovables,
            ISimulationStep step)
        {
            _size = box.WorldSize();
            _offset = box.WorldOffset();
            _platforms = platforms;
            _shovables = shovables;
            _step = step;
        }

        public bool TryGetRidden(Vector2 position, out Vector2 shift)
        {
            var bounds = UprightBounds(position);
            var top = float.NegativeInfinity;
            shift = Vector2.zero;

            for (var index = 0; index < _platforms.Count; index++)
            {
                var platform = _platforms[index];
                for (var box = 0; box < platform.BoxCount; box++)
                    Ride(platform.BoxAt(box).Area, platform.Shift, bounds, ref top, ref shift);
            }

            foreach (var body in _shovables.Keys)
                Ride(AreaOf(body), ShiftOf(body), bounds, ref top, ref shift);

            return !float.IsNegativeInfinity(top);
        }

        public Vector2 Scooped(Vector2 position, float skin, bool dropping)
        {
            var bounds = UprightBounds(position);
            var lift = 0f;

            for (var index = 0; index < _platforms.Count; index++)
            {
                var platform = _platforms[index];
                for (var box = 0; box < platform.BoxCount; box++)
                {
                    var moving = platform.BoxAt(box);
                    if (!(moving.IsOneWay && dropping))
                        lift = Mathf.Max(lift, LiftBy(moving.Area, platform.Shift, bounds, skin));
                }
            }

            foreach (var body in _shovables.Keys)
                lift = Mathf.Max(lift, LiftBy(AreaOf(body), ShiftOf(body), bounds, skin));

            return position + Vector2.up * lift;
        }

        private Vector2 ShiftOf(PhysicsBody body) =>
            body.linearVelocity * _step.Duration;

        private Rect UprightBounds(Vector2 position) =>
            new(position + _offset - _size / 2f, _size);

        private static void Ride(
            Rect area,
            Vector2 shift,
            Rect bounds,
            ref float top,
            ref Vector2 ridden)
        {
            var previous = new Rect(area.position - shift, area.size);
            if (!Riding(previous, bounds) || previous.yMax <= top)
                return;

            top = previous.yMax;
            ridden = shift;
        }

        private static float LiftBy(
            Rect area,
            Vector2 shift,
            Rect bounds,
            float skin)
        {
            var feet = bounds.yMin;
            var previousTop = area.yMax - shift.y;
            return shift.y > 0f && Overlapping(area, bounds) && feet >= previousTop - RideTolerance && feet < area.yMax + skin
                ? area.yMax + skin - feet
                : 0f;
        }

        private static Rect AreaOf(PhysicsBody body)
        {
            var bounds = body.GetAABB();
            return new Rect(bounds.lowerBound, bounds.upperBound - bounds.lowerBound);
        }

        private static bool Riding(Rect area, Rect bounds) =>
            Overlapping(area, bounds) && Mathf.Abs(bounds.yMin - area.yMax) <= RideTolerance;

        private static bool Overlapping(Rect area, Rect bounds) =>
            bounds.xMin < area.xMax && bounds.xMax > area.xMin;
    }
}
