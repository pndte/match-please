using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Movement;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class WeaponReleaseSpots
    {
        private readonly HeldWeaponPose _pose;
        private readonly PhysicsBody _physics;
        private readonly BodyBox _shape;
        private readonly WeaponReleaseConfig _config;
        private readonly PhysicsQuery.QueryFilter _filter;

        public WeaponReleaseSpots(
            HeldWeaponPose pose,
            PhysicsBody physics,
            BodyBox shape,
            WeaponReleaseConfig config)
        {
            _pose = pose;
            _physics = physics;
            _shape = shape;
            _config = config;
            _filter = new PhysicsQuery.QueryFilter(PhysicsMask.All, config.Blockers.AsPhysicsMask());
        }

        public RigidState ReleasedFrom(HoldPose hands) //TODO: в узком месте, где оружию нет места, сколько его ни оттягивай назад вдоль ствола, оно отпускается в центре рук, и физика выталкивает его как получится
        {
            var held = _pose.PoseIn(hands);
            var spot = FreeSpot(hands.Hold.center, held);
            return new RigidState(spot.Position, spot.Angle, Vector2.up * _config.DropSpeed, 0f);
        }

        private RigidState FreeSpot(Vector2 center, RigidState held)
        {
            var back = (Vector2)(Quaternion.Euler(0f, 0f, held.Angle) * Vector2.left);
            var reach = Vector2.Distance(center, held.Position) + _shape.WorldSize().x;
            for (var pulled = 0f; pulled <= reach; pulled += _config.PullStep)
            {
                var pose = new RigidState(held.Position + back * pulled, held.Angle, Vector2.zero, 0f);
                if (IsFree(pose))
                    return pose;
            }

            return new RigidState(center - OffsetAt(held.Angle), held.Angle, Vector2.zero, 0f);
        }

        private bool IsFree(RigidState pose)
        {
            var placed = new PhysicsTransform(pose.Position + OffsetAt(pose.Angle), new PhysicsRotate(pose.Angle * Mathf.Deg2Rad));
            var box = PolygonGeometry.CreateBox(_shape.WorldSize() - Vector2.one * (PhysicsSlop.Linear * 2f), 0f, placed, false);
            return _physics.world.IsFreeOfSolids(box, _filter, _physics);
        }

        private Vector2 OffsetAt(float angle) =>
            Quaternion.Euler(0f, 0f, angle) * _shape.WorldOffset();
    }
}
