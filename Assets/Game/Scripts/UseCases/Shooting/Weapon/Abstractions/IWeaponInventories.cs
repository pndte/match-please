using Bw.Entities.Infrastructure;
using Bw.UseCases.Character;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IWeaponInventories //TODO: Carriers и Drawn наружу только для чтения, чтобы запись с чужим лайфтаймом не пережила персонажа
    {
        public IViewableMap<IWeapon, IReadonlyCharacter> Carriers { get; }
        public IViewableBiMap<IReadonlyCharacter, IWeapon> Drawn { get; }

        public void Give(Lifetime lifetime, IReadonlyCharacter character, WeaponSlot slot, IWeapon weapon);
        public void Draw(IWeapon weapon);
        public void Drop(IWeapon weapon);
        public void PickUp(Lifetime lifetime, IReadonlyCharacter character, IWeapon weapon);
    }
}
