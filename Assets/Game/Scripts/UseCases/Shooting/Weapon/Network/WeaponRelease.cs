using System;
using Bw.Entities.Extensions;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class WeaponRelease
    {
        private readonly IRestartable<RigidState> _body;
        private readonly PhysicsBody _physics;
        private readonly BodyBox _shape;
        private readonly WeaponReleaseConfig _config;
        private readonly PhysicsQuery.QueryFilter _filter;

        public WeaponRelease(
            Lifetime lifetime,
            IWeaponHold hold,
            IRestartable<RigidState> body,
            PhysicsBody physics,
            BodyBox shape,
            WeaponReleaseConfig config)
        {
            _body = body;
            _physics = physics;
            _shape = shape;
            _config = config;
            _filter = new PhysicsQuery.QueryFilter(PhysicsMask.All, config.Blockers.AsPhysicsMask());

            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
            {
                var holder = Holder();
                heldLifetime.OnTermination(() => Release(holder));
            });
        }

        private void Release(BodyBox holder) //TODO: в узком месте, где оружию нет места, сколько его ни оттягивай назад вдоль ствола, оно отпускается в центре держателя, и физика выталкивает его как получится
        {
            var weapon = _shape.transform;
            var pose = ReleasePose(holder.WorldRect().center, new RigidState((Vector2)weapon.position, weapon.eulerAngles.z, Vector2.zero, 0f));
            _body.Restart(new RigidState(pose.Position, pose.Angle, Vector2.up * _config.DropSpeed, 0f));
        }

        private RigidState ReleasePose(Vector2 center, RigidState held)
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

        private BodyBox Holder()
        {
            var holder = _shape.transform.parent;
            if (holder == null || !holder.TryGetComponent<BodyBox>(out var box))
                throw new InvalidOperationException($"Weapon '{_shape.name}' is held only by a holder with a body box.");

            return box;
        }
    }
}
