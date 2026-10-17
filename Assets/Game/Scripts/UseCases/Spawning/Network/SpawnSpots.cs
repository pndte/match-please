using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Spawning.Network
{
    public sealed class SpawnSpots //TODO: свободного места нет ни у одной точки — персонаж встаёт на свою точку, даже если там стоит ящик, а тело из такого наложения не выталкивается
    {
        //TODO: мб конфиг, хз
        private const int ShiftsPerPoint = 8;
        private const float Tolerance = 0.005f;

        private readonly Transform[] _points;
        private readonly float _shift;
        private readonly PhysicsWorld _world;
        private readonly PhysicsQuery.QueryFilter _blockers;

        public SpawnSpots(
            Transform[] points,
            float shift,
            LayerMask blockers,
            IPhysicsSpace space)
        {
            _points = points;
            _shift = shift;
            _world = space.World;
            _blockers = new PhysicsQuery.QueryFilter(PhysicsMask.All, blockers.AsPhysicsMask());
        }

        public Vector3 SpotFor(BodyBox box, int turn)
        {
            var shifts = _shift > 0f ? ShiftsPerPoint : 1;
            for (var offset = 0; offset < _points.Length; offset++)
            {
                var point = PointOf(turn + offset);
                for (var attempt = 0; attempt < shifts; attempt++)
                {
                    var spot = point + Vector3.right * Random.Range(-_shift, _shift);
                    if (IsFree(box, spot))
                        return spot;
                }
            }

            return PointOf(turn);
        }

        private Vector3 PointOf(int turn) =>
            _points[(turn % _points.Length + _points.Length) % _points.Length].position;

        private bool IsFree(BodyBox box, Vector2 spot) =>
            _world.IsFree(PolygonGeometry.CreateBox(box.WorldSize() - Vector2.one * (Tolerance * 2f), 0f, new PhysicsTransform(spot + box.WorldOffset()), false), _blockers);
    }
}
