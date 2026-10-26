using Bw.Entities;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Abstractions.Inventory
{
    public interface IReadonlyWeaponCarriers
    {
        public IReadonlyViewableMap<IWeapon, ICharacter> ByWeapon { get; }
    }

    public interface IWeaponCarriers : IReadonlyWeaponCarriers
    {
        public void Add(Lifetime carriedLifetime, IWeapon weapon, ICharacter carrier);
    }
}
