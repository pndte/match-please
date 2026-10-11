using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IReadonlyWeapon
    {
        public ISource<WeaponShot> Fired { get; }
    }

    public interface IWeapon : IReadonlyWeapon
    {
        public bool Stowable { get; }
    }
}
