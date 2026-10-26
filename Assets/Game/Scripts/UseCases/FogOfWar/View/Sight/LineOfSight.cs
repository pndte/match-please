using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.FogOfWar.View.Sight
{
    public sealed class LineOfSight
    {
        private const float Grazing = 0.0005f;

        public int Count => _count;

        private static readonly IComparer<SightRay> ByAngle = new AngleOrder();

        private readonly PhysicsWorld _world;
        private readonly PhysicsQuery.QueryFilter _blockers;
        private readonly PhysicsMask _seeThrough;
        private readonly FogOfWarConfig _config;

        private SightRay[] _rays = new SightRay[1024];
        private int _count;

        public LineOfSight(PhysicsWorld world, PhysicsMask seeThrough, FogOfWarConfig config)
        {
            _world = world;
            _blockers = new PhysicsQuery.QueryFilter(PhysicsMask.All, new PhysicsMask(config.Blockers));
            _seeThrough = seeThrough;
            _config = config;
        }

        public SightRay Ray(int index) =>
            _rays[index];

        public void Trace(Vector2 eye, Rect area)
        {
            _count = 0;
            AimPastBlockers(eye, area);
            AimAt(eye, area, area.min);
            AimAt(eye, area, new Vector2(area.xMax, area.yMin));
            AimAt(eye, area, area.max);
            AimAt(eye, area, new Vector2(area.xMin, area.yMax));

            for (var index = 0; index < _config.FanRays; index++)
                Cast(eye, area, Mathf.PI * 2f * index / _config.FanRays - Mathf.PI);

            Array.Sort(_rays, 0, _count, ByAngle);
        }

        private void AimPastBlockers(Vector2 eye, Rect area)
        {
            using var inArea = _world.OverlapAABB(new PhysicsAABB(area.min, area.max), _blockers, Allocator.Temp);
            for (var index = 0; index < inArea.Length; index++)
            {
                var shape = inArea[index].shape;
                if (!IsSeeThrough(shape))
                    AimPastCorners(eye, area, shape);
            }
        }

        private void AimPastCorners(Vector2 eye, Rect area, PhysicsShape shape)
        {
            if (shape.shapeType != PhysicsShape.ShapeType.Polygon)
                return;

            var corners = shape.polygonGeometry.Transform(shape.transform).AsReadOnlySpan();
            for (var corner = 0; corner < corners.Length; corner++)
                AimPast(eye, area, corners[corner]);
        }

        private void AimPast(Vector2 eye, Rect area, Vector2 corner)
        {
            var toCorner = corner - eye;
            var angle = Mathf.Atan2(toCorner.y, toCorner.x);
            Cast(eye, area, angle - Grazing);
            Cast(eye, area, angle);
            Cast(eye, area, angle + Grazing);
        }

        private void AimAt(Vector2 eye, Rect area, Vector2 point)
        {
            var toPoint = point - eye;
            Cast(eye, area, Mathf.Atan2(toPoint.y, toPoint.x));
        }

        private void Cast(Vector2 eye, Rect area, float angle)
        {
            var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            var reach = Reach(eye, direction, area);
            Add(TryFindBlocker(eye, direction * reach, out var blocker)
                ? SightRay.Hit(Wrapped(angle), blocker.point, blocker.normal)
                : SightRay.Miss(Wrapped(angle), eye + direction * reach));
        }

        private bool TryFindBlocker(Vector2 eye, Vector2 reach, out PhysicsQuery.WorldCastResult nearest)
        {
            using var hits = _world.CastRay(new PhysicsQuery.CastRayInput(eye, reach), _blockers, PhysicsQuery.WorldCastMode.AllSorted, Allocator.Temp);
            for (var index = 0; index < hits.Length; index++)
            {
                if (IsSeeThrough(hits[index].shape))
                    continue;

                nearest = hits[index];
                return true;
            }

            nearest = default;
            return false;
        }

        private void Add(SightRay ray)
        {
            if (_count == _rays.Length)
                Array.Resize(ref _rays, _rays.Length * 2);

            _rays[_count++] = ray;
        }

        private bool IsSeeThrough(PhysicsShape shape) =>
            (shape.contactFilter.categories.bitMask & _seeThrough.bitMask) != 0;

        private static float Reach(Vector2 eye, Vector2 direction, Rect area) =>
            Mathf.Max(0f, Mathf.Min(Exit(eye.x, direction.x, area.xMin, area.xMax), Exit(eye.y, direction.y, area.yMin, area.yMax)));

        private static float Exit(
            float from,
            float step,
            float min,
            float max) =>
            step > 0f ? (max - from) / step : step < 0f ? (min - from) / step : float.PositiveInfinity;

        private static float Wrapped(float angle) =>
            angle > Mathf.PI ? angle - Mathf.PI * 2f : angle <= -Mathf.PI ? angle + Mathf.PI * 2f : angle;

        private sealed class AngleOrder : IComparer<SightRay>
        {
            public int Compare(SightRay left, SightRay right) =>
                left.Angle.CompareTo(right.Angle);
        }
    }
}
