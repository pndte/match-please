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
        private const float DropSpeed = 2f;
        private const float Tolerance = 0.005f;

        private readonly IRestartable<RigidState> _body;
        private readonly PhysicsBody _physics;
        private readonly BodyBox _shape;
        private readonly PhysicsQuery.QueryFilter _filter;

        public WeaponRelease(
            Lifetime lifetime,
            IWeaponHold hold,
            IRestartable<RigidState> body,
            PhysicsBody physics,
            BodyBox shape,
            DynamicBodyConfig config)
        {
            _body = body;
            _physics = physics;
            _shape = shape;
            _filter = new PhysicsQuery.QueryFilter(PhysicsMask.All, config.Contacts.AsPhysicsMask());

            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
            {
                var holder = Holder();
                heldLifetime.OnTermination(() => Release(holder));
            });
        }

        private void Release(BodyBox holder) //TODO: оружие длиннее держателя — отпущенное в центре держателя у стены, оно может торчать в стену, и физика выталкивает его как получится
        {
            var weapon = _shape.transform;
            var angle = weapon.eulerAngles.z;
            var held = (Vector2)weapon.position;
            var position = IsFree(held, angle) ? held : holder.WorldRect().center - OffsetAt(angle);
            _body.Restart(new RigidState(position, angle, Vector2.up * DropSpeed, 0f));
        }

        private bool IsFree(Vector2 position, float angle)
        {
            var pose = new PhysicsTransform(position + OffsetAt(angle), new PhysicsRotate(angle * Mathf.Deg2Rad));
            var box = PolygonGeometry.CreateBox(_shape.WorldSize() - Vector2.one * (Tolerance * 2f), 0f, pose, false);
            return _physics.world.IsFree(box, _filter, _physics);
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
