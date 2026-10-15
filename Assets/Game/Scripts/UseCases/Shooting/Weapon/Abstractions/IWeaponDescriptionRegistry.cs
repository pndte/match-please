using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IReadonlyWeaponDescriptionRegistry
    {
        public WeaponDescription DescriptionOf(IReadonlyWeapon weapon);
    }

    public interface IWeaponDescriptionRegistry : IReadonlyWeaponDescriptionRegistry
    {
        public void AddLifetimed(Lifetime lifetime, IReadonlyWeapon weapon, WeaponDescription description);
    }
}
