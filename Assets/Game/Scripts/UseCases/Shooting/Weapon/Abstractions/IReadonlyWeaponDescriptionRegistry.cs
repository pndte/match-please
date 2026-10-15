namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IReadonlyWeaponDescriptionRegistry
    {
        public WeaponDescription DescriptionOf(IReadonlyWeapon weapon);
    }
}
