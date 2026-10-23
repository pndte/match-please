using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Abstractions
{
    public interface IReadonlyWeaponKindRegistry
    {
        public WeaponKind KindOf(IWeapon weapon);
    }

    public interface IWeaponKindRegistry : IReadonlyWeaponKindRegistry
    {
        public void AddLifetimed(Lifetime lifetime, IWeapon weapon, WeaponKind kind);
    }
}
