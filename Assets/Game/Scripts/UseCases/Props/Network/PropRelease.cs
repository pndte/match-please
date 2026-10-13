using System;
using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Props.Network
{
    public sealed class PropRelease
    {
        private const float Skin = 0.015f;

        private readonly IWeapon _prop;
        private readonly IWeaponInventories _inventories;
        private readonly IRestartable<BodyState> _body;
        private readonly ThrownPropHits _hits;
        private readonly Transform _transform;
        private readonly BoxCollider2D _shape;
        private readonly PropThrowConfig _config;
        private readonly ContactFilter2D _blockers;
        private readonly List<RaycastHit2D> _found = new(8);

        private PropLaunch _launch = PropLaunch.Dropped;

        public PropRelease(
            Lifetime lifetime,
            IWeapon prop,
            IWeaponHold hold,
            IWeaponInventories inventories,
            IRestartable<BodyState> body,
            ThrownPropHits hits,
            Transform transform,
            BoxCollider2D shape,
            IBodyConfig bodyConfig,
            PropThrowConfig config)
        {
            _prop = prop;
            _inventories = inventories;
            _body = body;
            _hits = hits;
            _transform = transform;
            _shape = shape;
            _config = config;
            _blockers = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = bodyConfig.GroundLayer,
                useTriggers = false
            };

            prop.Fired.Advise(lifetime, shot => Throw(shot.Aim));
            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
            {
                var holder = Holder();
                heldLifetime.OnTermination(() => Release(holder));
            });
        }

        private void Throw(float aim) //TODO: бросок решает сервер — бросивший видит, как ящик уходит из рук, только через полный пинг, а полёт потом смотрит в интерполяции; предсказать бросок — показывать полёт свободного тела от рук сразу и сводить его со снимками
        {
            var radians = aim * Mathf.Deg2Rad;
            var velocity = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * _config.Speed;
            var thrower = _inventories.Carriers[_prop];
            _launch = PropLaunch.Thrown(thrower, velocity);
            _inventories.InventoryOf(thrower).DropAndDrawNext(_prop);
        }

        private void Release(Collider2D holder)
        {
            var launch = _launch;
            _launch = PropLaunch.Dropped;
            var position = ReleasePosition(holder);

            launch.Switch(
                () => _body.Restart(new BodyState(position, Vector2.zero, 0f, false)),
                (thrower, velocity) =>
                {
                    _body.Restart(new BodyState(position, new Vector2(0f, velocity.y), velocity.x, false));
                    _hits.Track(thrower);
                });
        }

        private Vector2 ReleasePosition(Collider2D holder)
        {
            var scale = (Vector2)_transform.lossyScale;
            var offset = Vector2.Scale(_shape.offset, scale);
            var from = (Vector2)holder.bounds.center;
            var path = (Vector2)_transform.position + offset - from;
            var distance = path.magnitude;
            if (distance == 0f)
                return from - offset;

            var direction = path / distance;
            var count = Physics2D.BoxCast(from, Vector2.Scale(_shape.size, scale), 0f, direction, _blockers, _found, distance);
            for (var index = 0; index < count; index++)
            {
                var hit = _found[index];
                if (hit.collider == _shape || hit.transform.root == holder.transform.root)
                    continue;

                return from + direction * Mathf.Max(0f, hit.distance - Skin) - offset;
            }

            return from + path - offset;
        }

        private Collider2D Holder()
        {
            var holder = _transform.parent;
            if (holder == null || !holder.TryGetComponent<Collider2D>(out var shape))
                throw new InvalidOperationException($"Prop '{_transform.name}' is held only by a holder with a collider.");

            return shape;
        }
    }
}
