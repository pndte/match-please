using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bw.UseCases.FogOfWar.View.Sight
{
    public sealed class LineOfSight
    {
        private const float Grazing = 0.0005f;

        public int Count => _count;

        private static readonly IComparer<SightRay> ByAngle = new AngleOrder();

        private readonly PhysicsScene2D _physics;
        private readonly ContactFilter2D _blockers;
        private readonly FogOfWarConfig _config;
        private readonly List<Collider2D> _inArea = new(64);
        private readonly PhysicsShapeGroup2D _shapes = new(8, 64);
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[32];

        private SightRay[] _rays = new SightRay[1024];
        private int _count;

        public LineOfSight(PhysicsScene2D physics, FogOfWarConfig config)
        {
            _physics = physics;
            _config = config;
            _blockers = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = config.Blockers,
                useTriggers = false
            };
        }

        public SightRay Ray(int index) =>
            _rays[index];

        public void Trace(Vector2 eye, Rect area)
        {
            _count = 0;
            _physics.OverlapArea(area.min, area.max, _blockers, _inArea);
            foreach (var collider in _inArea)
                if (!IsSeeThrough(collider))
                    AimPastCorners(eye, area, collider);

            AimAt(eye, area, area.min);
            AimAt(eye, area, new Vector2(area.xMax, area.yMin));
            AimAt(eye, area, area.max);
            AimAt(eye, area, new Vector2(area.xMin, area.yMax));

            for (var index = 0; index < _config.FanRays; index++)
                Cast(eye, area, Mathf.PI * 2f * index / _config.FanRays - Mathf.PI);

            Array.Sort(_rays, 0, _count, ByAngle);
        }

        private void AimPastCorners(Vector2 eye, Rect area, Collider2D collider)
        {
            collider.GetShapes(_shapes); //TODO: у круга и капсулы нет углов: их край ловят только лучи веера, тень от них ступенчатая
            var toWorld = _shapes.localToWorldMatrix;
            for (var shape = 0; shape < _shapes.shapeCount; shape++)
            {
                var corners = _shapes.GetShape(shape).vertexCount;
                for (var corner = 0; corner < corners; corner++)
                    AimPast(eye, area, toWorld.MultiplyPoint3x4(_shapes.GetShapeVertex(shape, corner)));
            }
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
            Add(TryFindBlocker(eye, direction, reach, out var blocker)
                ? SightRay.Hit(Wrapped(angle), blocker.point, blocker.normal)
                : SightRay.Miss(Wrapped(angle), eye + direction * reach));
        }

        private bool TryFindBlocker(Vector2 eye, Vector2 direction, float reach, out RaycastHit2D nearest)
        {
            nearest = default;
            var found = false;
            var count = _physics.Raycast(eye, direction, reach, _blockers, _hits);
            for (var index = 0; index < count; index++)
            {
                var hit = _hits[index];
                if (IsSeeThrough(hit.collider) || found && hit.distance >= nearest.distance)
                    continue;

                nearest = hit;
                found = true;
            }

            return found;
        }

        private void Add(SightRay ray)
        {
            if (_count == _rays.Length)
                Array.Resize(ref _rays, _rays.Length * 2);

            _rays[_count++] = ray;
        }

        private static bool IsSeeThrough(Collider2D collider) => //TODO: та же проверка доски, что CharacterBody.IsPlatform в движении: свести в одно место, когда у досок появится свой компонент
            collider.usedByEffector && collider.TryGetComponent(out PlatformEffector2D effector) && effector.enabled && effector.useOneWay;

        private static float Reach(Vector2 eye, Vector2 direction, Rect area) =>
            Mathf.Max(0f, Mathf.Min(Exit(eye.x, direction.x, area.xMin, area.xMax), Exit(eye.y, direction.y, area.yMin, area.yMax)));

        private static float Exit(float from, float step, float min, float max) =>
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
