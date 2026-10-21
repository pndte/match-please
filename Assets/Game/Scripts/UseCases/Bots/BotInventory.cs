using Bw.UseCases.Mortality;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;

namespace Bw.UseCases.Bots
{
    public sealed class BotInventory
    {
        private IWeaponInventory Inventory => _inventories.InventoryOf(_self);

        private readonly IReadonlyMortal _self;
        private readonly IWeaponInventories _inventories;

        public BotInventory(IReadonlyMortal self, IWeaponInventories inventories)
        {
            _self = self;
            _inventories = inventories;
        }

        public bool IsDrawn(WeaponKind kind) =>
            Inventory.TryFindDrawn(out var drawn) && Inventory.RulesOf(drawn).Kind == kind;

        public bool TryFindDrawable(WeaponKind kind, out IWeapon drawable)
        {
            var inventory = Inventory;
            foreach (var carried in inventory.Carried.Keys)
                if (inventory.RulesOf(carried).Kind == kind && inventory.Drawable(carried))
                {
                    drawable = carried;
                    return true;
                }

            drawable = default;
            return false;
        }

        public void Draw(IWeapon weapon) =>
            Inventory.SwitchTo(weapon);

        public bool TryPickUp(IWeapon weapon)
        {
            var inventory = Inventory;
            if (!inventory.Switchable())
                return false;

            inventory.PickUpSwapping(weapon);
            return true;
        }
    }
}
