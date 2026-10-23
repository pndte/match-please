namespace Bw.UseCases.Combat.Weapon.Network.Requests
{
    public readonly struct CarriedWeapon
    {
        public readonly ulong Weapon;
        public readonly byte Slot;

        public CarriedWeapon(ulong weapon, byte slot)
        {
            Weapon = weapon;
            Slot = slot;
        }
    }
}
