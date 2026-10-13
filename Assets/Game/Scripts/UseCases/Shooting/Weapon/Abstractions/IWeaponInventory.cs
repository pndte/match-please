using System.Collections.Generic;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IReadonlyWeaponInventory
    {
        public IReadOnlyList<WeaponSlot> Slots { get; }
        public IReadOnlyDictionary<IWeapon, WeaponSlot> Carried { get; }
        public IReadOnlyList<IWeapon> DrawOrder { get; }

        public WeaponKindRules RulesOf(IWeapon weapon);
        public bool TryFindDrawn(out IWeapon drawn);
    }

    public interface IWeaponInventory : IReadonlyWeaponInventory
    {
        public void Give(WeaponSlot slot, IWeapon weapon);
        public void Draw(IWeapon weapon);
        public void Drop(IWeapon weapon);
    }
}
