using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Extensions;

namespace Bw.UseCases.Combat.Hands.Extensions
{
    public static class HandsInventoryExtensions
    {
        public static bool TryFindTop(this IReadonlyWeaponInventory inventory, HandsSlot slot, out IWeapon top)
        {
            var (found, weapon) = slot.Switch(
                inventory,
                static _ => (false, default(IWeapon)),
                static (carried, weaponSlot) => (carried.TryFindTop(weaponSlot, out var held), held));
            top = weapon;
            return found;
        }
    }
}
