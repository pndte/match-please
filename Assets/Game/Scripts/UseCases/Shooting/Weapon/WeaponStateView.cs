using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class WeaponStateView<TState> : IStateView<TState> where TState : struct, IWeaponState
    {
        private readonly Transform _weapon;
        private readonly WeaponRotationConfig _config;

        private bool _drawn;
        private float _radius;

        public WeaponStateView(Lifetime lifetime, Transform weapon, IReadonlyWeaponDraw draw, WeaponRotationConfig config)
        {
            _weapon = weapon;
            _config = config;
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
            var hit = Physics2D.Raycast(
                holder.position,
                holder.TransformDirection(direction),
                _config.ObstacleDetectionDistance,
                _config.ObstacleLayerMask);

            return hit.collider != null ? _config.MinOrbitRadius : _config.MaxOrbitRadius;
        }
    }
}
