using Bw.UseCases.Shooting.Graphics;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon
{
    public class BulletTrailRenderer
    {
        private readonly IWeaponMuzzle _weaponMuzzle;
        private readonly IShotVfxPlayer _shotVfxPlayer;
        private readonly RaycastShootConfig _raycastConfig;

        public BulletTrailRenderer(
            Lifetime lifetime,
            IWeapon weapon,
            IWeaponMuzzle weaponMuzzle,
            IShotVfxPlayer shotVfxPlayer,
            RaycastShootConfig raycastConfig)
        {
            _weaponMuzzle = weaponMuzzle;
            _shotVfxPlayer = shotVfxPlayer;
            _raycastConfig = raycastConfig;
            weapon.OnShot.Advise(lifetime, HandleShot);
        }

        private void HandleShot(Vector3 mousePosition)
        {
            var origin = _weaponMuzzle.Transform.position;
            var direction = ((Vector2)(mousePosition - origin)).normalized;
            if (Mathf.Approximately(direction.sqrMagnitude, 0)) return;

            var hit = Physics2D.Raycast(origin, direction, _raycastConfig.MaxDistance, _raycastConfig.HitMask);

            Vector3 endPoint = hit.collider != null
                ? hit.point
                : origin + (Vector3)(direction * _raycastConfig.MaxDistance);

            _shotVfxPlayer.Play(_weaponMuzzle.Transform.position, endPoint).Forget();
        }
    }
}