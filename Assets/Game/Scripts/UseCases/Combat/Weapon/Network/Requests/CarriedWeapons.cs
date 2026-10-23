namespace Bw.UseCases.Combat.Weapon.Network.Requests
{
    public readonly struct CarriedWeapons
    {
        public readonly CarriedWeapon[] Weapons;

        public CarriedWeapons(CarriedWeapon[] weapons)
        {
            Weapons = weapons;
        }
    }
}
