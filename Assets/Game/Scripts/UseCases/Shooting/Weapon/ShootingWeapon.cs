using System;
using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class ShootingWeapon<TState> : IWeapon, ISimulation<TState>
        where TState : struct, IWeaponState, IEquatable<TState>
    {
        public IReadonlyProperty<TState> State => _state;
        public TState Previous { get; private set; }
        public ISource<TState> Stepped => _stepped;
        public ISource<WeaponShot> Fired => _fired;
        public bool Stowable => _state.Value.Stowable;

        private readonly ViewableProperty<TState> _state;
        private readonly Signal<TState> _stepped = new();
        private readonly Signal<WeaponShot> _fired = new();
        private readonly IWeaponSimulator<TState> _simulator;

        private float _triggerViewDelay;
        private float _triggerPredictedDelay;

        public ShootingWeapon(
            Lifetime lifetime,
            ISource<WeaponInput> inputs,
            IReadonlyWeaponDraw draw,
            IWeaponSimulator<TState> simulator,
            TState initial)
        {
            _simulator = simulator;
            Previous = initial;
            _state = new ViewableProperty<TState>(initial);

            draw.DrawnLifetime.WhenAlive(lifetime, drawnLifetime =>
            {
                inputs.Advise(drawnLifetime, Step);
                drawnLifetime.OnTermination(Interrupt);
            });
        }

        public void Apply(TState state) =>
            _state.Value = state;

        private void Step(WeaponInput input)
        {
            if (input.Trigger)
            {
                _triggerViewDelay = input.ViewDelay;
                _triggerPredictedDelay = input.PredictedDelay;
            }

            Previous = _state.Value;
            _state.Value = _simulator.Step(Previous, input);
            _stepped.Fire(_state.Value);

            if (_state.Value.Shots != Previous.Shots)
                _fired.Fire(new WeaponShot(_state.Value.Aim, _triggerViewDelay, _triggerPredictedDelay));
        }

        private void Interrupt() =>
            _state.Value = _simulator.Interrupted(_state.Value);
    }
}
