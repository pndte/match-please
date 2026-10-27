using Bw.Entities;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Abstractions
{
    public interface IReadonlyWeaponStates<TState> where TState : struct
    {
        public IReadonlyViewableMap<IWeapon, IReadonlySimulation<TState>> ByWeapon { get; }
    }

    public interface IWeaponStates<TState> : IReadonlyWeaponStates<TState> where TState : struct
    {
        public void Add(Lifetime lifetime, IWeapon weapon, IReadonlySimulation<TState> simulation);
    }
}
