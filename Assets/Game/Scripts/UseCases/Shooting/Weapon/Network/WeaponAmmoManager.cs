using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public class WeaponAmmoManager
    {
        private readonly IWeapon _weapon;
        private readonly IReloader _reloader;
        private readonly ShootingWeaponConfig _config;
        private readonly IAmmo _ammo;
            
        public WeaponAmmoManager(
            Lifetime lifetime, 
            IWeapon weapon, 
            IReloader reloader,
            ShootingWeaponConfig config, 
            IAmmo ammo)
        {
            _weapon = weapon;
            _reloader = reloader;
            _config = config;
            _ammo = ammo;
            HandleResources(lifetime);
        }

        private void HandleResources(Lifetime lifetime)
        {
            _weapon.OnShot.Advise(lifetime, _ => _ammo.Value--);
            _reloader.AdviseReloadComplete(lifetime, () => _ammo.Value = _config.AmmoSettings.Max);
        }
    }
}