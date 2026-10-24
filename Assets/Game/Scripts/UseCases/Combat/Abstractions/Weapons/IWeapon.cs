using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Abstractions.Weapons
{
    public interface IReadonlyWeapon
    {
        public ISource<WeaponShot> Fired { get; }
    }

    public interface IWeapon : IReadonlyWeapon, ISteppable<WeaponInput>
    {
        public bool Stowable { get; }
        public bool Rooting { get; }
    }
}
