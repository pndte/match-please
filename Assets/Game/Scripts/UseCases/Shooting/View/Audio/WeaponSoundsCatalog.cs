using System;
using Bw.UseCases.Shooting.Weapon;

namespace Bw.UseCases.Shooting.View.Audio
{
    public sealed class WeaponSoundsCatalog
    {
        private readonly WeaponSoundsConfig[] _weapons;

        public WeaponSoundsCatalog(WeaponSoundsConfig[] weapons)
        {
            _weapons = weapons;
        }

        public WeaponSoundsConfig For(ShootingWeaponConfig weapon)
        {
            foreach (var sounds in _weapons)
                if (sounds.Weapon == weapon)
                    return sounds;

            throw new InvalidOperationException($"SoundSettings has no weapon sounds for '{weapon.name}': add an entry with this weapon config to its weapon list.");
        }
    }
}
