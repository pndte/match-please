using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public class ShootingWeaponServerHandler //TODO: ренейм и релокейт
    {
        private readonly IReadonlyWeapon _shootingWeapon;
        private readonly IReloader _reloader;
        private readonly ShootingWeaponConfig _config;
        private readonly IAmmo _ammo;
            
        public ShootingWeaponServerHandler(
            Lifetime lifetime, 
            IReadonlyWeapon shootingWeapon, 
            IReloader reloader,
            ShootingWeaponConfig config, 
            IAmmo ammo)
        {
            _shootingWeapon = shootingWeapon;
            _reloader = reloader;
            _config = config;
            _ammo = ammo;
            HandleResources(lifetime);
        }

        private void HandleResources(Lifetime lifetime)
        {
            _shootingWeapon.Shot.Advise(lifetime, _ => _ammo.Value--);
            _reloader.ReloadComplete.Advise(lifetime, _ => _ammo.Value = _config.AmmoSettings.Max);
        }
    }
}