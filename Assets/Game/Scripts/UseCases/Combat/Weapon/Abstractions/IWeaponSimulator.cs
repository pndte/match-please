using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.UseCases.Combat.Weapon.Abstractions
{
    public interface IWeaponSimulator<TState> : ISimulator<WeaponInput, TState> where TState : struct
    {
        public TState Interrupted(TState state);
        public TState Rested(TState state);
    }
}
