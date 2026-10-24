using System;
using System.Collections.Generic;
using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.UseCases.Combat.Abstractions.Inventory
{
    public readonly struct InventoryContent : IEquatable<InventoryContent>
    {
        public static readonly InventoryContent Empty = new(Array.Empty<InventoryItem>());

        public readonly IReadOnlyList<InventoryItem> Items;

        public InventoryContent(IReadOnlyList<InventoryItem> items)
        {
            Items = items;
        }

        public bool IsListed(IWeapon weapon, WeaponSlot slot)
        {
            for (var index = 0; index < Items.Count; index++)
                if (Items[index].Weapon == weapon && Items[index].Slot == slot)
                    return true;

            return false;
        }

        public bool Equals(InventoryContent other)
        {
            if (Items.Count != other.Items.Count)
                return false;

            for (var index = 0; index < Items.Count; index++)
                if (!other.IsListed(Items[index].Weapon, Items[index].Slot))
                    return false;

            return true;
        }

        public override bool Equals(object obj) =>
            obj is InventoryContent other && Equals(other);

        public override int GetHashCode() =>
            Items.Count;
    }

    public readonly struct InventoryItem
    {
        public readonly IWeapon Weapon;
        public readonly WeaponSlot Slot;

        public InventoryItem(IWeapon weapon, WeaponSlot slot)
        {
            Weapon = weapon;
            Slot = slot;
        }
    }
}
