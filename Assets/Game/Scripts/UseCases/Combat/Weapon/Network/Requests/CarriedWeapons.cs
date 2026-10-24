using System;

namespace Bw.UseCases.Combat.Weapon.Network.Requests
{
    public readonly struct CarriedWeapons : IEquatable<CarriedWeapons>
    {
        public readonly CarriedWeapon[] Weapons;

        public CarriedWeapons(CarriedWeapon[] weapons)
        {
            Weapons = weapons;
        }

        public bool Equals(CarriedWeapons other)
        {
            if (Weapons.Length != other.Weapons.Length)
                return false;

            foreach (var weapon in Weapons)
                if (Array.IndexOf(other.Weapons, weapon) < 0)
                    return false;

            return true;
        }

        public override bool Equals(object obj) =>
            obj is CarriedWeapons other && Equals(other);

        public override int GetHashCode() =>
            Weapons.Length;
    }
}
