using System;
using Bw.Entities;
using Bw.UseCases.Mortality;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.UseCases.Combat.Weapon.Doubles
{
    public sealed class FakeMortal : IReadonlyMortal
    {
        public IReadonlyHealth Health => throw new NotSupportedException("The fake mortal has no health.");
        public IReadonlyProperty<LifeState> State => _state;
        public Vitals Current => new(default, _state.Value);
        public ISource<Vitals> Applied => _applied;

        private readonly ViewableProperty<LifeState> _state;
        private readonly Signal<Vitals> _applied = new();

        public FakeMortal(LifeState state = LifeState.Alive)
        {
            _state = new ViewableProperty<LifeState>(state);
        }

        public void Die() =>
            _state.Value = LifeState.Dead;

        public void Live() =>
            _state.Value = LifeState.Alive;
    }
}
