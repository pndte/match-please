using System;
using Bw.Entities;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Mortality;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Shooting.Melee
{
    public sealed class MeleeTracer : IShotTracer
    {
        public int Pellets => 1;

        private readonly Transform _weapon;
        private readonly MeleeWeaponConfig _config;
        private readonly PhysicsWorld _world;
        private readonly PhysicsQuery.QueryFilter _filter;

        public MeleeTracer(Transform weapon, MeleeWeaponConfig config, IPhysicsSpace space)
        {
            _weapon = weapon;
            _config = config;
            _world = space.World;
            _filter = new PhysicsQuery.QueryFilter(PhysicsMask.All, config.HitMask.AsPhysicsMask());
        }

        public ShotRay Aim(float aim)
        {
            var holder = Holder();
            return new ShotRay(
                holder.position,
                holder.position,
                holder.TransformDirection(Quaternion.Euler(0f, 0f, aim) * Vector3.right));
        }

        public ShotRay Pellet(ShotRay ray, int pellet)
        {
            if (pellet != 0)
                throw new ArgumentOutOfRangeException(nameof(pellet), pellet, $"Melee weapon '{_weapon.name}' strikes along one ray.");

            return ray;
        }

        public ShotTrace Cast(ShotRay ray)
        {
            var reach = ray.Direction * _config.Reach;
            var fist = new CircleGeometry { center = ray.Pivot, radius = _config.Radius };
            using var hits = _world.CastGeometry(fist, reach, _filter, PhysicsQuery.WorldCastMode.AllSorted, Allocator.Temp);
            return hits.TryFindStruck(Striker(), out var hit)
                ? ShotTrace.Struck(ray.Pivot, hit, ray.Direction)
                : ShotTrace.Missed(ray.Pivot, ray.Pivot + reach);
        }

        private IReadonlyMortal Striker()
        {
            if (!Holder().root.TryGetComponent<IHolder<IReadonlyMortal>>(out var striker))
                throw new InvalidOperationException($"Melee weapon '{_weapon.name}' traces a strike only for a mortal holder: the holder's root holds no mortal.");

            return striker.Value;
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
