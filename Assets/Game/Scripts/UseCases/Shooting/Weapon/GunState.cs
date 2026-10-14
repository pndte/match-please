using System;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Shooting.Weapon
{
    public readonly struct GunState : IWeaponState, IEquatable<GunState>
    {
        public readonly float Aim;
        public readonly int Ammo;
        public readonly int CooldownTicks;
        public readonly int ReloadTicks;
        public readonly int Shots;

        float IWeaponState.Aim => Aim;
        int IWeaponState.Shots => Shots;
        bool IWeaponState.Stowable => true;

        public GunState(
            float aim,
            int ammo,
            int cooldownTicks,
            int reloadTicks,
            int shots)
        {
            Aim = aim;
            Ammo = ammo;
            CooldownTicks = cooldownTicks;
            ReloadTicks = reloadTicks;
            Shots = shots;
        }

        public bool Equals(GunState other) =>
            Aim.Equals(other.Aim)
            && Ammo == other.Ammo
            && CooldownTicks == other.CooldownTicks
            && ReloadTicks == other.ReloadTicks
            && Shots == other.Shots;
    }
}
