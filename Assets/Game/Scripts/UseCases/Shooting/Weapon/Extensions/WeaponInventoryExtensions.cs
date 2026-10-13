using System;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class WeaponInventoryExtensions
    {
        public static bool TryFindIn(this IReadonlyWeaponInventory inventory, WeaponSlot slot, bool droppable, out IWeapon weapon)
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

        public static bool InHands(this IReadonlyWeaponInventory inventory, IWeapon weapon) =>
            inventory.TryFindDrawn(out var drawn) && drawn == weapon;

        public static bool Switchable(this IReadonlyWeaponInventory inventory) =>
            !inventory.TryFindDrawn(out var drawn) || drawn.Stowable;

        public static bool Drawable(this IReadonlyWeaponInventory inventory, IWeapon weapon) =>
            inventory.Carried.ContainsKey(weapon) && !inventory.Covered(weapon) && !inventory.InHands(weapon) && inventory.Switchable();

        public static IWeapon ReplacedBy(this IReadonlyWeaponInventory inventory, IWeapon weapon)
        {
            var kind = inventory.RulesOf(weapon).Kind;
            for (var index = inventory.DrawOrder.Count - 1; index >= 0; index--)
            {
                var drawn = inventory.DrawOrder[index];
                if (Replaceable(inventory, drawn, kind))
                    return drawn;
            }

            foreach (var slot in inventory.Slots)
                if (slot.IsFor(kind) && inventory.TryFindIn(slot, droppable: true, out var first))
                    return first;

            throw new InvalidOperationException($"No droppable {kind.ToString()} weapon is carried to be replaced.");
        }

        public static bool TryFindLast(this IReadonlyWeaponInventory inventory, out IWeapon last)
        {
            for (var index = inventory.DrawOrder.Count - 1; index >= 0; index--)
            {
                last = inventory.DrawOrder[index];
                if (inventory.Drawable(last))
                    return true;
            }

            last = null;
            return false;
        }

        public static bool TryFindScrolled(this IReadonlyWeaponInventory inventory, int direction, out IWeapon scrolled)
        {
            if (direction != 1 && direction != -1)
                throw new ArgumentOutOfRangeException(
                    nameof(direction), $"A scroll moves one slot forward (1) or back (-1), not {direction.ToString()}.");

            var count = inventory.Slots.Count;
            var start = inventory.TryFindDrawn(out var drawn) ? IndexOf(inventory, inventory.Carried[drawn]) : direction > 0 ? -1 : count;
            for (var offset = 1; offset <= count; offset++)
            {
                var index = ((start + direction * offset) % count + count) % count;
                if (inventory.TryFindTop(inventory.Slots[index], out scrolled) && inventory.Drawable(scrolled))
                    return true;
            }

            scrolled = null;
            return false;
        }

        public static bool TryFindNext(this IReadonlyWeaponInventory inventory, WeaponSlot vacated, out IWeapon next) =>
            inventory.TryFindTop(vacated, out next)
            || TryFindLatestUncovered(inventory, out next)
            || TryFindFirstTop(inventory, out next);

        public static void SwitchTo(this IWeaponInventory inventory, IWeapon weapon)
        {
            if (!inventory.Drawable(weapon))
                throw new InvalidOperationException("Only a carried, uncovered weapon out of hands is switched to, and only while the hands can let go.");

            if (inventory.TryFindDrawn(out var drawn) && inventory.RulesOf(drawn).HandsOnly)
                inventory.Drop(drawn);

            inventory.Draw(weapon);
        }

        public static void PickUp(this IWeaponInventory inventory, IWeapon weapon)
        {
            RequirePickable(inventory, weapon);
            inventory.Give(inventory.FreeSlotFor(weapon), weapon);
            inventory.SwitchTo(weapon);
        }

        public static void PickUpSwapping(this IWeaponInventory inventory, IWeapon weapon)
        {
            RequirePickable(inventory, weapon);
            if (!inventory.TryFindFreeSlot(weapon, out _))
                inventory.Drop(inventory.ReplacedBy(weapon));

            inventory.PickUp(weapon);
        }

        public static void DropAndDrawNext(this IWeaponInventory inventory, IWeapon weapon)
        {
            if (!inventory.InHands(weapon))
                throw new InvalidOperationException("Only the weapon in hands is dropped with the next one drawn.");

            var vacated = inventory.Carried[weapon];
            inventory.Drop(weapon);
            if (inventory.TryFindNext(vacated, out var next))
                inventory.Draw(next);
        }

        private static bool TryFindLatestUncovered(IReadonlyWeaponInventory inventory, out IWeapon latest)
        {
            for (var index = inventory.DrawOrder.Count - 1; index >= 0; index--)
            {
                latest = inventory.DrawOrder[index];
                if (!inventory.Covered(latest))
                    return true;
            }

            latest = null;
            return false;
        }

        private static bool TryFindFirstTop(IReadonlyWeaponInventory inventory, out IWeapon top)
        {
            foreach (var slot in inventory.Slots)
                if (inventory.TryFindTop(slot, out top))
                    return true;

            top = null;
            return false;
        }

        private static int IndexOf(IReadonlyWeaponInventory inventory, WeaponSlot slot)
        {
            for (var index = 0; index < inventory.Slots.Count; index++)
                if (inventory.Slots[index] == slot)
                    return index;

            throw new InvalidOperationException("The slot is not in this inventory's layout.");
        }

        private static bool Replaceable(IReadonlyWeaponInventory inventory, IWeapon weapon, WeaponKind kind) =>
            inventory.RulesOf(weapon).Droppable && inventory.Carried[weapon].IsFor(kind);

        private static void RequirePickable(IReadonlyWeaponInventory inventory, IWeapon weapon)
        {
            if (inventory.Carried.ContainsKey(weapon))
                throw new InvalidOperationException("The weapon is in this inventory already.");

            if (!inventory.RulesOf(weapon).Droppable)
                throw new InvalidOperationException("An undroppable weapon is never picked up: it never lies on the floor.");

            if (!inventory.Switchable())
                throw new InvalidOperationException("Nothing is picked up while the weapon in hands can't be put away.");
        }
    }
}
