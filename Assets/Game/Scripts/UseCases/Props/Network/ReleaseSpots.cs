using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Movement;
using Bw.UseCases.Props.Extensions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Props.Network
{
    public sealed class ReleaseSpots
    {
        private readonly PhysicsWorld _world;
        private readonly PhysicsBody _body;
        private readonly Vector2 _size;
        private readonly Vector2 _offset;
        private readonly PhysicsQuery.QueryFilter _filter;
        private readonly PhysicsQuery.QueryFilter _walls;
        private readonly HeldPropConfig _config;

        public ReleaseSpots(
            IPhysicsSpace space,
            PhysicsBody body,
            BodyBox shape,
            DynamicBodyConfig bodyConfig,
            HeldPropConfig config)
        {
            _world = space.World;
            _body = body;
            _size = shape.WorldSize();
            _offset = shape.WorldOffset();
            _filter = new PhysicsQuery.QueryFilter(PhysicsMask.All, bodyConfig.Contacts.AsPhysicsMask());
            _walls = new PhysicsQuery.QueryFilter(PhysicsMask.All, config.Walls.AsPhysicsMask());
            _config = config;
        }

        public RigidState Nearest(IReadonlyCharacter holder, GripPose held) //TODO: в замкнутом месте (стены с боков, пол и потолок вплотную) свободного места нет ни на одном кольце — предмет отпускается в руках, и физика выталкивает его как получится
        {
            var inHands = new RigidState(held.Center - Turned(_offset, held.Angle), held.Angle, Vector2.zero, 0f);
            if (IsFree(inHands, holder))
                return inHands;

            var oval = holder.Hands.Pose.Hold;
            for (var ring = 0; ring < _config.ReleaseRings; ring++)
            {
                if (ring > 0 && TryGetFree(oval, holder, held.Angle, ring, out var straight))
                    return straight;

                for (var turn = _config.ReleaseAngleStep; turn <= 180f; turn += _config.ReleaseAngleStep)
                    if (TryGetFree(oval, holder, held.Angle + turn, ring, out var spot) || TryGetFree(oval, holder, held.Angle - turn, ring, out spot))
                        return spot;
            }

            return inHands;
        }

        private bool TryGetFree(
            Rect oval,
            IReadonlyCharacter holder,
            float angle,
            int ring,
            out RigidState spot)
        {
            var center = oval.center + _config.HeldOffset(angle, oval.size / 2f + _size * ring, _size / 2f);
            spot = new RigidState(center - Turned(_offset, angle), angle, Vector2.zero, 0f);
            return IsReachable(oval.center, center) && IsFree(spot, holder);
        }

        private bool IsFree(RigidState pose, IReadonlyCharacter holder)
        {
            var placed = new PhysicsTransform(pose.Position + Turned(_offset, pose.Angle), new PhysicsRotate(pose.Angle * Mathf.Deg2Rad));
            using var found = _world.OverlapGeometry(PolygonGeometry.CreateBox(_size - Vector2.one * (PhysicsSlop.Linear * 2f), 0f, placed, false), _filter, Allocator.Temp);
            foreach (var overlap in found)
                if (!overlap.shape.body.Equals(_body) && !overlap.shape.IsOneWay() && !IsOwnedBy(overlap.shape, holder))
                    return false;

            return true;
        }

        private bool IsReachable(Vector2 from, Vector2 to)
        {
            using var hits = _world.CastRay(new PhysicsQuery.CastRayInput(from, to - from), _walls, PhysicsQuery.WorldCastMode.AllSorted, Allocator.Temp);
            foreach (var hit in hits)
                if (hit.normal != Vector2.zero && !hit.shape.IsOneWay())
                    return false;

            return true;
        }

        private static bool IsOwnedBy(PhysicsShape shape, IReadonlyCharacter holder) =>
            shape.TryGetMortal(out var mortal) && mortal == holder.Mortal;

        private static Vector2 Turned(Vector2 offset, float angle) =>
            Quaternion.Euler(0f, 0f, angle) * offset;
    }
}
