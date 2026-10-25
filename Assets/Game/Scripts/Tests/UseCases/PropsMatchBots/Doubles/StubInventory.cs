using System.Collections.Generic;
using Bw.Entities;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.Tests.UseCases.PropsMatchBots.Doubles
{
    public sealed class StubInventory : IWeaponInventory
    {
        public IReadOnlyList<WeaponSlot> Slots { get; }
        public IReadonlyViewableMap<IWeapon, WeaponSlot> Carried => _carried;
        public IReadOnlyList<IWeapon> Dropped => _dropped;

        private readonly BwViewableMap<IWeapon, WeaponSlot> _carried = new();
        private readonly Dictionary<IWeapon, WeaponKindRules> _rules = new();
        private readonly List<IWeapon> _dropped = new();

        public StubInventory(params WeaponSlot[] slots)
        {
            Slots = slots;
        }

        public void Carry(WeaponSlot slot, IWeapon weapon, WeaponKindRules rules)
        {
            _rules[weapon] = rules;
            _carried.Add(weapon, slot);
        }

        public WeaponKindRules RulesOf(IWeapon weapon) =>
            _rules[weapon];

        public void Give(WeaponSlot slot, IWeapon weapon) =>
            _carried.Add(weapon, slot);

        public void Drop(IWeapon weapon)
        {
            _dropped.Add(weapon);
            _carried.Remove(weapon);
        }
    }
}
