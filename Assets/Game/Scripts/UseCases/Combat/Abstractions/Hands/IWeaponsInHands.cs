using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Abstractions.Hands
{
    public interface IWeaponsInHands //TODO: ByWeapon наружу открыт на запись, как и у носителей: запись с чужим лайфтаймом пережила бы руки
    {
        public IViewableMap<IWeapon, IReadonlyCharacter> ByWeapon { get; }
    }
}
