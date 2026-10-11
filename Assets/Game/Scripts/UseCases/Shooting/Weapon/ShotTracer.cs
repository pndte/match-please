using System;
using System.Collections.Generic;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class ShotTracer : IShotTracer
    {
        public int Pellets => _pellets.Count;

        private readonly Transform _weapon;
        private readonly IWeaponMuzzle _muzzle;
        private readonly RaycastShootConfig _config;
        private readonly PelletsConfig _pellets;
        private readonly ContactFilter2D _filter;
        private readonly List<RaycastHit2D> _hits = new(8);

        public ShotTracer(Transform weapon, IWeaponMuzzle muzzle, RaycastShootConfig config, PelletsConfig pellets)
        {
            _weapon = weapon;
            _muzzle = muzzle;
            _config = config;
            _pellets = pellets;
            _filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = config.HitMask,
                useTriggers = false
            };
        }

        public ShotRay Aim(float aim)
        {
            var holder = Holder();
            var rotation = Quaternion.Euler(0f, 0f, aim);
            var muzzleOffset = _muzzle.Transform.localPosition;
            var radius = ((Vector2)_weapon.localPosition).magnitude;

            return new ShotRay(
                holder.position,
                holder.TransformPoint(rotation * new Vector3(radius + muzzleOffset.x, muzzleOffset.y)),
                holder.TransformDirection(rotation * Vector3.right));
        }

        public ShotRay Pellet(ShotRay ray, int pellet)
        {
            if (pellet < 0 || pellet >= _pellets.Count)
                throw new ArgumentOutOfRangeException(nameof(pellet), pellet, $"Weapon '{_weapon.name}' fires {_pellets.Count.ToString()} pellets a shot.");

            return ray.Turned(_pellets.AngleOf(pellet));
        }

        public ShotTrace Cast(ShotRay ray)
        {
            var shooter = Holder().root;

            Physics2D.Linecast(ray.Pivot, ray.Muzzle, _filter, _hits);
            if (_hits.TryFindForeign(shooter, out var blocking))
                return new ShotTrace(blocking.point, blocking.point, blocking);

            Physics2D.Raycast(ray.Muzzle, ray.Direction, _filter, _hits, _config.MaxDistance);
            return _hits.TryFindForeign(shooter, out var hit)
                ? new ShotTrace(ray.Muzzle, hit.point, hit)
                : new ShotTrace(ray.Muzzle, ray.Muzzle + ray.Direction * _config.MaxDistance, hit);
        }

        private Transform Holder()
        {
            var holder = _weapon.parent;
            if (holder == null)
                throw new InvalidOperationException($"Weapon '{_weapon.name}' traces a shot only while it is held.");

            return holder;
        }
    }
}
