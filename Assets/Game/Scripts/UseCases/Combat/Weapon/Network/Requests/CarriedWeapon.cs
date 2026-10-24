using System;

namespace Bw.UseCases.Combat.Weapon.Network.Requests
{
    public readonly struct CarriedWeapon : IEquatable<CarriedWeapon>
    {
        public readonly ulong Weapon;
        public readonly byte Slot;

        public CarriedWeapon(ulong weapon, byte slot)
        {
            Weapon = weapon;
            Slot = slot;
        }

        public bool Equals(CarriedWeapon other) =>
            Weapon == other.Weapon && Slot == other.Slot;

        public override bool Equals(object obj) =>
            obj is CarriedWeapon other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Weapon, Slot);
    }
}
