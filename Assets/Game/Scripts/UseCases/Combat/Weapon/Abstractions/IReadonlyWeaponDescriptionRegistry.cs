using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.UseCases.Combat.Weapon.Abstractions
{
    public interface IReadonlyWeaponDescriptionRegistry
    {
        public WeaponDescription DescriptionOf(IReadonlyWeapon weapon);
    }
}
