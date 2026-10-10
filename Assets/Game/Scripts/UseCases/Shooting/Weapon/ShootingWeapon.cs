using Bw.Entities.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class ShootingWeapon : IWeapon
    {
        public IReadonlyProperty<WeaponState> State => _state;
        public WeaponState Previous { get; private set; }
        public ISource<WeaponState> Stepped => _stepped;
        public ISource<WeaponShot> Fired => _fired;

        private readonly ViewableProperty<WeaponState> _state;
        private readonly Signal<WeaponState> _stepped = new();
        private readonly Signal<WeaponShot> _fired = new();
        private readonly IWeaponSimulator _simulator;

        private float _triggerViewDelay;

        public ShootingWeapon(
            Lifetime lifetime,
            ISource<WeaponInput> inputs,
            IReadonlyWeaponDraw draw,
            IWeaponSimulator simulator,
            WeaponState initial)
        {
            _simulator = simulator;
            Previous = initial;
            _state = new ViewableProperty<WeaponState>(initial);

            draw.DrawnLifetime.WhenAlive(lifetime, drawnLifetime =>
            {
                inputs.Advise(drawnLifetime, Step);
                drawnLifetime.OnTermination(Interrupt);
            });
        }

        public void Apply(WeaponState state) =>
            _state.Value = state;

        private void Step(WeaponInput input)
        {
            if (input.Trigger)
                _triggerViewDelay = input.ViewDelay;

            Previous = _state.Value;
            _state.Value = _simulator.Step(Previous, input);
            _stepped.Fire(_state.Value);

            if (_state.Value.Shots != Previous.Shots)
                _fired.Fire(new WeaponShot(_state.Value.Aim, _triggerViewDelay));
        }

        private void Interrupt() =>
            _state.Value = _simulator.Interrupted(_state.Value);
    }
}
