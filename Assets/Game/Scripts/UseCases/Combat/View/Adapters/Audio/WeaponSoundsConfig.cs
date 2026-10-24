using System;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.View.Audio;

namespace Bw.UseCases.Combat.View.Adapters.Audio
{
    [Serializable]
    public sealed class WeaponSoundsConfig : GunSounds
    {
        public ShootingWeaponConfig Weapon;
    }
}
