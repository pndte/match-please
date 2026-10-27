using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon
{
    public sealed class WeaponStates<TState> : IWeaponStates<TState> where TState : struct
    {
        public IReadonlyViewableMap<IWeapon, IReadonlySimulation<TState>> ByWeapon => _byWeapon;

        private readonly BwViewableMap<IWeapon, IReadonlySimulation<TState>> _byWeapon = new();

        public void Add(Lifetime lifetime, IWeapon weapon, IReadonlySimulation<TState> simulation) =>
            _byWeapon.AddLifetimed(lifetime, weapon, simulation);
    }
}
