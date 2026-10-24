using System;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Hands.Extensions;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Shooting.Extensions;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Combat.Weapon.Extensions;
using Bw.UseCases.Mortality;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Combat.Shooting
{
    public sealed class ShotTracer : IShotTracer
    {
        public int Pellets => _pellets.Count;

        private readonly IWeapon _weapon;
        private readonly IWeaponsInHands _inHands;
        private readonly WeaponMuzzle _muzzle;
        private readonly RaycastShootConfig _config;
        private readonly PelletsConfig _pellets;
        private readonly PhysicsWorld _world;
        private readonly PhysicsQuery.QueryFilter _filter;

        public ShotTracer(
            IWeapon weapon,
            IWeaponsInHands inHands,
            WeaponMuzzle muzzle,
            RaycastShootConfig config,
            PelletsConfig pellets,
            IPhysicsSpace space)
        {
            _weapon = weapon;
            _inHands = inHands;
            _muzzle = muzzle;
            _config = config;
            _pellets = pellets;
            _world = space.World;
            _filter = new PhysicsQuery.QueryFilter(PhysicsMask.All, config.HitMask.AsPhysicsMask());
        }

        public ShotRay Aim(float aim) =>
            _muzzle.RayFrom(_inHands.HolderOf(_weapon).Hands.Pose, aim);

        public ShotRay Pellet(ShotRay ray, int pellet)
        {
            if (pellet < 0 || pellet >= _pellets.Count)
                throw new ArgumentOutOfRangeException(nameof(pellet), pellet, $"Weapon config '{_config.name}' fires {_pellets.Count.ToString()} pellets a shot.");

            return ray.Turned(_pellets.AngleOf(pellet));
        }

        public ShotTrace Cast(ShotRay ray)
        {
            var shooter = _inHands.HolderOf(_weapon).Mortal;
            using var blocking = Hits(ray.Pivot, ray.Muzzle - ray.Pivot);
            if (blocking.TryFindStruck(shooter, out var wall))
                return ShotTrace.Struck(wall.point, wall, ray.Direction);

            var reach = ray.Direction * _config.MaxDistance;
            using var hits = Hits(ray.Muzzle, reach);
            return hits.TryFindStruck(shooter, out var hit)
                ? ShotTrace.Struck(ray.Muzzle, hit, ray.Direction)
                : ShotTrace.Missed(ray.Muzzle, ray.Muzzle + reach);
        }

        private NativeArray<PhysicsQuery.WorldCastResult> Hits(Vector2 from, Vector2 translation) =>
            _world.CastRay(new PhysicsQuery.CastRayInput(from, translation), _filter, PhysicsQuery.WorldCastMode.AllSorted, Allocator.Temp);
    }
}
