using System;
using Bw.Entities.Extensions;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Props.Network
{
    public sealed class PropRelease
    {
        private readonly IWeapon _prop;
        private readonly IWeaponInventories _inventories;
        private readonly IRestartable<RigidState> _body;
        private readonly ThrownPropHits _hits;
        private readonly Transform _transform;
        private readonly ReleaseSpots _spots;
        private readonly PropThrowConfig _config;

        private PropLaunch _launch = PropLaunch.Dropped;

        public PropRelease(
            Lifetime lifetime,
            IWeapon prop,
            IWeaponHold hold,
            IWeaponInventories inventories,
            IRestartable<RigidState> body,
            ThrownPropHits hits,
            Transform transform,
            ReleaseSpots spots,
            PropThrowConfig config)
        {
            _prop = prop;
            _inventories = inventories;
            _body = body;
            _hits = hits;
            _transform = transform;
            _spots = spots;
            _config = config;

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

        private void Release(BodyBox holder)
        {
            var launch = _launch;
            _launch = PropLaunch.Dropped;
            var position = _spots.Nearest(holder.WorldRect(), _transform.position);

            launch.Switch(
                () => _body.Restart(new RigidState(position, 0f, Vector2.zero, 0f)),
                (thrower, velocity) =>
                {
                    _body.Restart(new RigidState(position, 0f, velocity, -Mathf.Sign(velocity.x) * _config.Spin));
                    _hits.Track(thrower);
                });
        }

        private BodyBox Holder()
        {
            var holder = _transform.parent;
            if (holder == null || !holder.TryGetComponent<BodyBox>(out var box))
                throw new InvalidOperationException($"Prop '{_transform.name}' is held only by a holder with a body box.");

            return box;
        }
    }
}
