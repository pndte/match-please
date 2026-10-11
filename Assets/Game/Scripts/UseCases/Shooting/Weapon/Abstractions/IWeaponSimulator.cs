using Bw.Entities.Simulation;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IWeaponSimulator<TState> : ISimulator<WeaponInput, TState> where TState : struct
    {
        public TState Interrupted(TState state);
    }
}
