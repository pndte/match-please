using System;
using Bw.UseCases.Shooting.Melee;

namespace Bw.UseCases.Shooting.View.Adapters.Audio
{
    public sealed class MeleeSoundsCatalog
    {
        private readonly MeleeSoundsConfig[] _weapons;

        public MeleeSoundsCatalog(MeleeSoundsConfig[] weapons)
        {
            _weapons = weapons;
        }

        public MeleeSoundsConfig For(MeleeWeaponConfig weapon)
        {
            foreach (var sounds in _weapons)
                if (sounds.Weapon == weapon)
                    return sounds;

            throw new InvalidOperationException($"SoundSettings has no melee sounds for '{weapon.name}': add an entry with this melee config to its melee list.");
        }
    }
}
