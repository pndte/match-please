using Bw.Entities.Infrastructure;
using Bw.UseCases.Character;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IHeldWeaponCollection //TODO: подбор с пола: запрос подбора и серверная проверка (у персонажа нет оружия, оружие ничьё, рядом) — сюда же, методами коллекции; ByCharacter тогда наружу только для чтения, чтобы запись с чужим лайфтаймом не пережила персонажа
    {
        public IViewableBiMap<ICharacter, IWeapon> ByCharacter { get; }
    }
}
