namespace Bw.UseCases.Combat.Weapon.Network.Requests
{
    public readonly struct WeaponPickup
    {
        public readonly ulong Weapon;

        public WeaponPickup(ulong weapon)
        {
            Weapon = weapon;
        }
    }
}
