using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;


namespace Bw.UseCases.Combat.Weapon.Extensions
{
    public static class WeaponSlotExtensions
    {
        public static bool IsFor(this WeaponSlot slot, WeaponKind kind)
        {
            foreach (var accepted in slot.Kinds)
                if (accepted == kind)
                    return true;

            return false;
        }
    }
}
