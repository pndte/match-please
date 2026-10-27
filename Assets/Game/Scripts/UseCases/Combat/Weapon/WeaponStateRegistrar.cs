using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon
{
    public sealed class WeaponStateRegistrar<TState> where TState : struct
    {
        public WeaponStateRegistrar(
            Lifetime lifetime,
            IWeapon weapon,
            IReadonlySimulation<TState> simulation,
            IWeaponStates<TState> states)
        {
            states.Add(lifetime, weapon, simulation);
        }
    }
}
