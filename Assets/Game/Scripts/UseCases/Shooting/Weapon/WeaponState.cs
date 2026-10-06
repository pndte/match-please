using System;

namespace Bw.UseCases.Shooting.Weapon
{
    public readonly struct WeaponState : IEquatable<WeaponState>
    {
        public readonly float Aim;
        public readonly int Ammo;
        public readonly int CooldownTicks;
        public readonly int ReloadTicks;
        public readonly int Shots;

        public WeaponState(float aim, int ammo, int cooldownTicks, int reloadTicks, int shots)
        {
            Aim = aim;
            Ammo = ammo;
            CooldownTicks = cooldownTicks;
            ReloadTicks = reloadTicks;
            Shots = shots;
        }

        public bool Equals(WeaponState other) =>
            Aim.Equals(other.Aim)
            && Ammo == other.Ammo
            && CooldownTicks == other.CooldownTicks
            && ReloadTicks == other.ReloadTicks
            && Shots == other.Shots;
    }
}
