using System;
using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement;
using Bw.UseCases.Props.Extensions;
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

        private void Throw(float aim)
        {
            var thrower = _inventories.Carriers[_prop];
            _launch = PropLaunch.Thrown(thrower, aim);
            _inventories.InventoryOf(thrower).DropAndDrawNext(_prop);
        }

        private void Release(HoldOval holder)
        {
            var launch = _launch;
            _launch = PropLaunch.Dropped;
            var released = _spots.Nearest(holder, new RigidState(_transform.position, _transform.eulerAngles.z, Vector2.zero, 0f));

            launch.Switch(
                () => _body.Restart(released),
                (thrower, aim) =>
                {
                    _body.Restart(_config.Thrown(released, aim));
                    _hits.Track(thrower);
                });
        }

        private HoldOval Holder()
        {
            var holder = _transform.parent;
            if (holder == null || !holder.TryGetComponent<HoldOval>(out var oval))
                throw new InvalidOperationException($"Prop '{_transform.name}' is held only by a holder with a hold oval.");

            return oval;
        }
    }
}
