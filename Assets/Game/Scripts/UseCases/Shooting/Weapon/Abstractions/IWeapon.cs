using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IReadonlyWeapon : IReadonlySimulation<WeaponState>
    {
        public ISource<WeaponState> Fired { get; }
    }

    public interface IWeapon : IReadonlyWeapon, ISimulation<WeaponState>
    {
    }
}
