using Bw.Entities.Simulation;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IWeaponSimulator : ISimulator<WeaponInput, WeaponState>
    {
        public WeaponState Interrupted(WeaponState state);
    }
}
