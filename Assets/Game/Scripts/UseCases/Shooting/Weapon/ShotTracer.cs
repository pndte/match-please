using System;
using System.Collections.Generic;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class ShotTracer : IShotTracer
    {
        private readonly Transform _weapon;
        private readonly IWeaponMuzzle _muzzle;
        private readonly RaycastShootConfig _config;
        private readonly ContactFilter2D _filter;
        private readonly List<RaycastHit2D> _hits = new(8);

        public ShotTracer(Transform weapon, IWeaponMuzzle muzzle, RaycastShootConfig config)
        {
            _weapon = weapon;
            _muzzle = muzzle;
            _config = config;
            _filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = config.HitMask,
                useTriggers = false
            };
        }

        public ShotTrace Trace(float aim)
        {
            var holder = _weapon.parent;
            if (holder == null)
                throw new InvalidOperationException($"Weapon '{_weapon.name}' traces a shot only while it is held.");

            var rotation = Quaternion.Euler(0f, 0f, aim);
            var muzzleOffset = _muzzle.Transform.localPosition;
            var radius = ((Vector2)_weapon.localPosition).magnitude;
            var pivot = (Vector2)holder.position;
            var muzzle = (Vector2)holder.TransformPoint(rotation * new Vector3(radius + muzzleOffset.x, muzzleOffset.y));
            var direction = (Vector2)holder.TransformDirection(rotation * Vector3.right);

            Physics2D.Linecast(pivot, muzzle, _filter, _hits);
            if (TryFindForeignHit(holder.root, out var blocking))
                return new ShotTrace(blocking.point, blocking.point, blocking);

            Physics2D.Raycast(muzzle, direction, _filter, _hits, _config.MaxDistance);
            return TryFindForeignHit(holder.root, out var hit)
                ? new ShotTrace(muzzle, hit.point, hit)
                : new ShotTrace(muzzle, muzzle + direction * _config.MaxDistance, hit);
        }

        private bool TryFindForeignHit(Transform shooter, out RaycastHit2D hit)
        {
            for (var index = 0; index < _hits.Count; index++)
            {
                if (_hits[index].transform.root == shooter)
                    continue;

                hit = _hits[index];
                return true;
            }

            hit = default;
            return false;
        }
    }
}
