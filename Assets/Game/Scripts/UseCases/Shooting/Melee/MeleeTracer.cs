using System;
using System.Collections.Generic;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using UnityEngine;

namespace Bw.UseCases.Shooting.Melee
{
    public sealed class MeleeTracer : IShotTracer
    {
        private readonly Transform _weapon;
        private readonly MeleeWeaponConfig _config;
        private readonly ContactFilter2D _filter;
        private readonly List<RaycastHit2D> _hits = new(8);

        public MeleeTracer(Transform weapon, MeleeWeaponConfig config)
        {
            _weapon = weapon;
            _config = config;
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
            return new ShotRay(
                holder.position,
                holder.position,
                holder.TransformDirection(Quaternion.Euler(0f, 0f, aim) * Vector3.right));
        }

        public ShotTrace Cast(ShotRay ray)
        {
            Physics2D.CircleCast(ray.Pivot, _config.Radius, ray.Direction, _filter, _hits, _config.Reach);
            return _hits.TryFindForeign(Holder().root, out var hit)
                ? new ShotTrace(ray.Pivot, hit.point, hit)
                : new ShotTrace(ray.Pivot, ray.Pivot + ray.Direction * _config.Reach, hit);
        }

        private Transform Holder()
        {
            var holder = _weapon.parent;
            if (holder == null)
                throw new InvalidOperationException($"Melee weapon '{_weapon.name}' traces a strike only while it is held.");

            return holder;
        }
    }
}
