using Bw.Entities.Extensions;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class WeaponStateView<TState> : IStateView<TState> where TState : struct, IWeaponState
    {
        private readonly Transform _weapon;
        private readonly WeaponRotationConfig _config;
        private readonly PhysicsWorld _world;
        private readonly PhysicsQuery.QueryFilter _obstacles;

        private bool _drawn;
        private float _radius;

        public WeaponStateView(
            Lifetime lifetime,
            Transform weapon,
            IReadonlyWeaponDraw draw,
            WeaponRotationConfig config,
            IPhysicsSpace space)
        {
            _weapon = weapon;
            _config = config;
            _world = space.World;
            _obstacles = new PhysicsQuery.QueryFilter(PhysicsMask.All, config.ObstacleLayerMask.AsPhysicsMask());
            _radius = config.MaxOrbitRadius;

            draw.DrawnLifetime.WhenAlive(lifetime, drawnLifetime =>
            {
                _drawn = true;
                drawnLifetime.OnTermination(() => _drawn = false);
            });
        }

        public void Show(TState from, TState to, float progress)
        {
            if (!_drawn)
                return;

            var rotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(from.Aim, to.Aim, progress));
            var direction = (Vector2)(rotation * Vector3.right);
            _radius = Mathf.Lerp(_radius, TargetRadius(direction), _config.RadiusChangeSpeed * Time.deltaTime);

            _weapon.localPosition = direction * _radius;
            _weapon.localRotation = rotation;
        }

        private float TargetRadius(Vector2 direction)
        {
            var holder = _weapon.parent;
            var reach = (Vector2)holder.TransformDirection(direction) * _config.ObstacleDetectionDistance;
            using var obstacles = _world.CastRay(new PhysicsQuery.CastRayInput(holder.position, reach), _obstacles, PhysicsQuery.WorldCastMode.Closest, Allocator.Temp);
            return obstacles.Length > 0 ? _config.MinOrbitRadius : _config.MaxOrbitRadius;
        }
    }
}
