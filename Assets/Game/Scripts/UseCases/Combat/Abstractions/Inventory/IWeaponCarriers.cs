using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Abstractions.Inventory
{
    public interface IWeaponCarriers //TODO: ByWeapon наружу только для чтения, чтобы запись с чужим лайфтаймом не пережила персонажа
    {
        public IViewableMap<IWeapon, ICharacter> ByWeapon { get; }
    }
}
