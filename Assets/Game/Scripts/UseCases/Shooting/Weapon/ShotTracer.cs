using System;
using Bw.Entities;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Mortality;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class ShotTracer : IShotTracer
    {
        public int Pellets => _pellets.Count;

        private readonly Transform _weapon;
        private readonly IWeaponMuzzle _muzzle;
        private readonly RaycastShootConfig _config;
        private readonly PelletsConfig _pellets;
        private readonly PhysicsWorld _world;
        private readonly PhysicsQuery.QueryFilter _filter;

        public ShotTracer(
            Transform weapon,
            IWeaponMuzzle muzzle,
            RaycastShootConfig config,
            PelletsConfig pellets,
            IPhysicsSpace space)
        {
            _weapon = weapon;
            _muzzle = muzzle;
            _config = config;
            _pellets = pellets;
            _world = space.World;
            _filter = new PhysicsQuery.QueryFilter(PhysicsMask.All, config.HitMask.AsPhysicsMask());
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
            var shooter = Shooter();
            using var blocking = Hits(ray.Pivot, ray.Muzzle - ray.Pivot);
            if (blocking.TryFindForeign(shooter, out var wall))
                return ShotTrace.Struck(wall.point, wall, ray.Direction);

            var reach = ray.Direction * _config.MaxDistance;
            using var hits = Hits(ray.Muzzle, reach);
            return hits.TryFindForeign(shooter, out var hit)
                ? ShotTrace.Struck(ray.Muzzle, hit, ray.Direction)
                : ShotTrace.Missed(ray.Muzzle, ray.Muzzle + reach);
        }

        private NativeArray<PhysicsQuery.WorldCastResult> Hits(Vector2 from, Vector2 translation) =>
            _world.CastRay(new PhysicsQuery.CastRayInput(from, translation), _filter, PhysicsQuery.WorldCastMode.AllSorted, Allocator.Temp);

        private IReadonlyMortal Shooter()
        {
            if (!Holder().root.TryGetComponent<IHolder<IReadonlyMortal>>(out var shooter))
                throw new InvalidOperationException($"Weapon '{_weapon.name}' traces a shot only for a mortal holder: the holder's root holds no mortal.");

            return shooter.Value;
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
