using System;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Hands.Extensions;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Combat.Weapon.Extensions;
using Bw.UseCases.Mortality;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Combat.Melee
{
    public sealed class MeleeTracer : IShotTracer
    {
        public int Pellets => 1;

        private readonly IWeapon _weapon;
        private readonly IWeaponsInHands _inHands;
        private readonly MeleeWeaponConfig _config;
        private readonly PhysicsWorld _world;
        private readonly PhysicsQuery.QueryFilter _filter;

        public MeleeTracer(
            IWeapon weapon,
            IWeaponsInHands inHands,
            MeleeWeaponConfig config,
            IPhysicsSpace space)
        {
            _weapon = weapon;
            _inHands = inHands;
            _config = config;
            _world = space.World;
            _filter = new PhysicsQuery.QueryFilter(PhysicsMask.All, config.HitMask.AsPhysicsMask());
        }

        public ShotRay Aim(float aim)
        {
            var pivot = _inHands.HolderOf(_weapon).Hands.Pose.Position;
            return new ShotRay(pivot, pivot, Quaternion.Euler(0f, 0f, aim) * Vector3.right);
        }

        public ShotRay Pellet(ShotRay ray, int pellet)
        {
            if (pellet != 0)
                throw new ArgumentOutOfRangeException(nameof(pellet), pellet, $"Melee config '{_config.name}' strikes along one ray.");

            return ray;
        }

        public ShotTrace Cast(ShotRay ray)
        {
            var reach = ray.Direction * _config.Reach;
            var fist = new CircleGeometry { center = ray.Pivot, radius = _config.Radius };
            using var hits = _world.CastGeometry(fist, reach, _filter, PhysicsQuery.WorldCastMode.AllSorted, Allocator.Temp);
            return hits.TryFindStruck(_inHands.HolderOf(_weapon).Mortal, out var hit)
                ? ShotTrace.Struck(ray.Pivot, hit, ray.Direction)
                : ShotTrace.Missed(ray.Pivot, ray.Pivot + reach);
        }
    }
}
