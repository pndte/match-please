using Bw.Entities.Infrastructure;
using Bw.UseCases.Character;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IWeaponInventories //TODO: подбор с пола: запрос подбора и серверная проверка (слот Primary свободен, оружие ничьё, рядом) — сюда же, методом реестра; Carriers и Drawn тогда наружу только для чтения, чтобы запись с чужим лайфтаймом не пережила персонажа
    {
        public IViewableMap<IWeapon, IReadonlyCharacter> Carriers { get; }
        public IViewableBiMap<IReadonlyCharacter, IWeapon> Drawn { get; }

        public void Give(Lifetime lifetime, IReadonlyCharacter character, WeaponSlot slot, IWeapon weapon);
        public void Draw(IWeapon weapon);
        public void Drop(IWeapon weapon);
    }
}
