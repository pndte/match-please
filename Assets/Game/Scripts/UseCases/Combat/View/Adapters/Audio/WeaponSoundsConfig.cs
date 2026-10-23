using System;
using Bw.UseCases.Combat.View.Audio;
using Bw.UseCases.Combat.Weapon;

namespace Bw.UseCases.Combat.View.Adapters.Audio
{
    [Serializable]
    public sealed class WeaponSoundsConfig : GunSounds
    {
        public ShootingWeaponConfig Weapon;
    }
}
