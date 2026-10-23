using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;

namespace Bw.UseCases.Combat.Weapon
{
    public sealed class GuardedWeaponSimulator<TState> : IWeaponSimulator<TState> where TState : struct
    {
        private readonly IWeaponSimulator<TState> _simulator;

        public GuardedWeaponSimulator(IWeaponSimulator<TState> simulator)
        {
            _simulator = simulator;
        }

        public TState Step(TState state, WeaponInput input) =>
            _simulator.Step(input.Guarded ? _simulator.Interrupted(state) : state, input);

        public TState Interrupted(TState state) =>
            _simulator.Interrupted(state);

        public TState Rested(TState state) =>
            _simulator.Rested(state);
    }
}
