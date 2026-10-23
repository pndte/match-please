using System.Collections.Generic;
using Bw.Entities;
using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.UseCases.Combat.Abstractions.Inventory
{
    public interface IReadonlyWeaponInventory
    {
        public IReadOnlyList<WeaponSlot> Slots { get; }
        public IReadonlyViewableMap<IWeapon, WeaponSlot> Carried { get; }

        public WeaponKindRules RulesOf(IWeapon weapon);
    }

    public interface IWeaponInventory : IReadonlyWeaponInventory
    {
        public void Give(WeaponSlot slot, IWeapon weapon);
        public void Drop(IWeapon weapon);
    }
}
