using System;
using System.Collections.Generic;
using Bw.Entities;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.Tests.UseCases.Combat.Weapon.Doubles
{
    public sealed class RecordingInventory : IWeaponInventory
    {
        public IReadOnlyList<WeaponSlot> Slots => _slots;
        public IReadonlyViewableMap<IWeapon, WeaponSlot> Carried => _carried;
        public IReadOnlyList<IWeapon> Dropped => _dropped;

        private readonly WeaponSlot[] _slots;
        private readonly BwViewableMap<IWeapon, WeaponSlot> _carried = new();
        private readonly List<IWeapon> _dropped = new();

        public RecordingInventory(params WeaponSlot[] slots)
        {
            _slots = slots;
        }

        public WeaponKindRules RulesOf(IWeapon weapon) =>
            new() { Kind = WeaponKind.Gun, Droppable = true };

        public void Give(WeaponSlot slot, IWeapon weapon) =>
            _carried.Add(weapon, slot);

        public void Drop(IWeapon weapon)
        {
            if (!_carried.Remove(weapon))
                throw new InvalidOperationException("The weapon is not in this inventory.");

            _dropped.Add(weapon);
        }
    }
}
