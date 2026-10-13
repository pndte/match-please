using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class WeaponInventoriesExtensions
    {
        public static bool TryFindInventoryCarrying(this IWeaponInventories inventories, IWeapon weapon, out IWeaponInventory inventory)
        {
            if (!inventories.Carriers.TryGetValue(weapon, out var carrier))
            {
                inventory = null;
                return false;
            }

            inventory = inventories.InventoryOf(carrier);
            return true;
        }
    }
}
