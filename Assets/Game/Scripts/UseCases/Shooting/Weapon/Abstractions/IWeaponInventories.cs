using Bw.Entities.Infrastructure;
using Bw.UseCases.Mortality;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IWeaponInventories //TODO: Carriers и Drawn наружу только для чтения, чтобы запись с чужим лайфтаймом не пережила персонажа
    {
        public IViewableMap<IWeapon, IReadonlyMortal> Carriers { get; }
        public IViewableBiMap<IReadonlyMortal, IWeapon> Drawn { get; }

        public IWeaponInventory InventoryOf(IReadonlyMortal character);
        public void Open(Lifetime lifetime, IReadonlyMortal character);
    }
}
