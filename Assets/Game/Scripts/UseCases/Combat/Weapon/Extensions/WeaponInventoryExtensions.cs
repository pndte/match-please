using System;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.UseCases.Combat.Weapon.Extensions
{
    public static class WeaponInventoryExtensions
    {
        public static bool TryFindIn(
            this IReadonlyWeaponInventory inventory,
            WeaponSlot slot,
            bool droppable,
            out IWeapon weapon)
        {
            foreach (var (carried, place) in inventory.Carried)
            {
                if (place != slot || inventory.RulesOf(carried).Droppable != droppable)
                    continue;

                weapon = carried;
                return true;
            }

            weapon = null;
            return false;
        }

        public static bool TryFindTop(this IReadonlyWeaponInventory inventory, WeaponSlot slot, out IWeapon top) =>
            inventory.TryFindIn(slot, droppable: true, out top) || inventory.TryFindIn(slot, droppable: false, out top);

        public static bool TryFindFreeSlot(this IReadonlyWeaponInventory inventory, IWeapon weapon, out WeaponSlot free)
        {
            var rules = inventory.RulesOf(weapon);
            foreach (var slot in inventory.Slots)
            {
                if (!slot.IsFor(rules.Kind) || inventory.TryFindIn(slot, rules.Droppable, out _))
                    continue;

                free = slot;
                return true;
            }

            free = null;
            return false;
        }

        public static WeaponSlot FreeSlotFor(this IReadonlyWeaponInventory inventory, IWeapon weapon)
        {
            if (!inventory.TryFindFreeSlot(weapon, out var free))
                throw new InvalidOperationException($"Every slot for {inventory.RulesOf(weapon).Kind.ToString()} weapons is taken.");

            return free;
        }

        public static bool Covered(this IReadonlyWeaponInventory inventory, IWeapon weapon) =>
            !inventory.RulesOf(weapon).Droppable && inventory.TryFindIn(inventory.Carried[weapon], droppable: true, out _);

        public static int IndexOf(this IReadonlyWeaponInventory inventory, WeaponSlot slot)
        {
            for (var index = 0; index < inventory.Slots.Count; index++)
                if (inventory.Slots[index] == slot)
                    return index;

            throw new InvalidOperationException("The slot is not in this inventory's layout.");
        }

        public static IWeapon ReplacedBy(this IReadonlyWeaponInventory inventory, IWeapon weapon)
        {
            var kind = inventory.RulesOf(weapon).Kind;
            foreach (var slot in inventory.Slots)
                if (slot.IsFor(kind) && inventory.TryFindIn(slot, droppable: true, out var replaced))
                    return replaced;

            throw new InvalidOperationException($"No droppable {kind.ToString()} weapon is carried to be replaced.");
        }

        public static void PickUp(this IWeaponInventory inventory, IWeapon weapon)
        {
            RequirePickable(inventory, weapon);
            inventory.Give(inventory.FreeSlotFor(weapon), weapon);
        }

        public static void PickUpSwapping(this IWeaponInventory inventory, IWeapon weapon)
        {
            RequirePickable(inventory, weapon);
            if (!inventory.TryFindFreeSlot(weapon, out _))
                inventory.Drop(inventory.ReplacedBy(weapon));

            inventory.PickUp(weapon);
        }

        private static void RequirePickable(IReadonlyWeaponInventory inventory, IWeapon weapon)
        {
            if (inventory.Carried.ContainsKey(weapon))
                throw new InvalidOperationException("The weapon is in this inventory already.");

            if (!inventory.RulesOf(weapon).Droppable)
                throw new InvalidOperationException("An undroppable weapon is never picked up: it never lies on the floor.");
        }
    }
}
