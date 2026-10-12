using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Mortality
{
    public sealed class Mortal : IMortal
    {
        public IReadonlyHealth Health => _health;
        public IReadonlyProperty<LifeState> State => _state;
        public ISource<Vitals> Applied => _applied;
        public Vitals Current => new(_health.State.Value.WithoutChange(), _state.Value);

        private readonly IHealth _health;
        private readonly ViewableProperty<LifeState> _state = new(LifeState.Alive);
        private readonly Signal<Vitals> _applied = new();

        public Mortal(IHealth health)
        {
            _health = health;
        }

        public void Apply(Vitals vitals)
        {
            if (_state.Value == LifeState.Dead && vitals.State == LifeState.Alive)
                throw new InvalidOperationException("The dead can't come back to life.");

            _health.Apply(vitals.Health);
            _state.Value = vitals.State;
            _applied.Fire(vitals);
        }
    }
}
