using System;
using Bw.UseCases.Shooting.View.Audio;
using Bw.UseCases.Shooting.Weapon;

namespace Bw.UseCases.Shooting.View.Adapters.Audio
{
    [Serializable]
    public sealed class WeaponSoundsConfig : GunSounds
    {
        public ShootingWeaponConfig Weapon;
    }
}
