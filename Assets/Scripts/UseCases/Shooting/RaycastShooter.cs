using Bw.UseCases.Character;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting
{
    public class RaycastShooter
    {
        private readonly IWeaponMuzzle _weaponMuzzle;
        private readonly RaycastShootConfig _raycastConfig;
        private readonly ShootingWeaponConfig _weaponConfig;

        public RaycastShooter(
            Lifetime lifetime,
            IWeapon weapon,
            IWeaponMuzzle weaponMuzzle,
            RaycastShootConfig raycastConfig,
            ShootingWeaponConfig weaponConfig
            )
        {
            _weaponMuzzle = weaponMuzzle;
            _raycastConfig = raycastConfig;
            _weaponConfig = weaponConfig;

            weapon.Shot.Advise(lifetime, HandleShot);
        }

        private void HandleShot(Vector3 mouseWorldPosition)
        {
            var origin = _weaponMuzzle.Transform.position;
            var direction = ((Vector2)(mouseWorldPosition - origin)).normalized;
            if (direction.sqrMagnitude < 0.0001f) return;

            var hit = Physics2D.Raycast(origin, direction, _raycastConfig.MaxDistance, _raycastConfig.HitMask);

            if (hit.collider != null && hit.collider.TryGetComponent<ICharacter>(out var character))
            {
                character.Health.Value -= _weaponConfig.Damage;
            }
        }
    }
}