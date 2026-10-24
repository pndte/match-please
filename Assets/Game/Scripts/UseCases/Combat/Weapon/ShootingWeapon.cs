using System;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon
{
    public sealed class ShootingWeapon<TState> : IWeapon, IWeaponFeed, IReadonlySimulation<TState>, IRestorable<TState>, ISteppable
        where TState : struct, IWeaponState, IEquatable<TState>
    {
        public IReadonlyProperty<TState> State => _state;
        public TState Previous { get; private set; }
        public ISource<TState> Stepped => _stepped;
        public ISource<WeaponShot> Fired => _fired;
        public ISource<WeaponInput> Fed => _fed;
        public bool Stowable => _state.Value.Stowable;
        public bool Rooting => _state.Value.Rooting;

        private readonly ViewableProperty<TState> _state;
        private readonly Signal<TState> _stepped = new();
        private readonly Signal<WeaponShot> _fired = new();
        private readonly Signal<WeaponInput> _fed = new();
        private readonly IWeaponSimulator<TState> _simulator;

        public ShootingWeapon(IWeaponSimulator<TState> simulator, TState initial)
        {
            _simulator = simulator;
            Previous = initial;
            _state = new ViewableProperty<TState>(initial);
        }

        public void Apply(TState state) =>
            _state.Value = state;

        public void Step(WeaponInput input)
        {
            _fed.Fire(input);
            Previous = _state.Value;
            _state.Value = _simulator.Step(Previous, input);
            _stepped.Fire(_state.Value);

            if (_state.Value.Shots != Previous.Shots)
                _fired.Fire(new WeaponShot(_state.Value.Aim));
        }

        public void Step()
        {
            var rested = _simulator.Rested(_state.Value);
            Previous = _state.Value;
            if (rested.Equals(_state.Value))
                return;

            _state.Value = rested;
            _stepped.Fire(rested);
        }
    }
}
