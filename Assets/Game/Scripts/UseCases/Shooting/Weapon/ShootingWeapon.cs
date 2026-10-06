using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
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
        private readonly ISimulator<WeaponInput, WeaponState> _simulator;

        public ShootingWeapon(
            Lifetime lifetime,
            ISource<WeaponInput> inputs,
            IWeaponHold hold,
            ISimulator<WeaponInput, WeaponState> simulator,
            AmmoConfig ammoConfig)
        {
            _simulator = simulator;
            Previous = new WeaponState(0f, ammoConfig.OnSpawnValue, 0, 0, 0);
            _state = new ViewableProperty<WeaponState>(Previous);

            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
            {
                inputs.Advise(heldLifetime, Step);
                heldLifetime.OnTermination(InterruptReload);
            });
        }

        public void Apply(WeaponState state) =>
            _state.Value = state;

        private void Step(WeaponInput input)
        {
            Previous = _state.Value;
            _state.Value = _simulator.Step(Previous, input);
            _stepped.Fire(_state.Value);

            if (_state.Value.Shots != Previous.Shots)
                _fired.Fire(new WeaponShot(_state.Value.Aim, input.ViewDelay));
        }

        private void InterruptReload() =>
            _state.Value = _state.Value.WithoutReload();
    }
}
