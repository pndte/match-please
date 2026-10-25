using Bw.Entities;
using Bw.UseCases.Mortality;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.UseCases.PropsMatchBots.Doubles
{
    public sealed class StubMortal : IReadonlyMortal
    {
        public IReadonlyHealth Health => _health;
        public IReadonlyProperty<LifeState> State => _state;
        public Vitals Current => new(_health.State.Value, _state.Value);
        public ISource<Vitals> Applied => _applied;

        private readonly StubHealth _health;
        private readonly ViewableProperty<LifeState> _state = new(LifeState.Alive);
        private readonly Signal<Vitals> _applied = new();

        public StubMortal(float max = 100f)
        {
            _health = new StubHealth(max);
        }

        public void Hurt(float current) =>
            _health.Set(current);

        public void Die() =>
            _state.Value = LifeState.Dead;

        private sealed class StubHealth : IReadonlyHealth
        {
            public float Max { get; }
            public IReadonlyProperty<HealthState> State => _state;
            public ISource<float> Changed => _changed;

            private readonly ViewableProperty<HealthState> _state;
            private readonly Signal<float> _changed = new();

            public StubHealth(float max)
            {
                Max = max;
                _state = new ViewableProperty<HealthState>(new HealthState(max, 0f));
            }

            public void Set(float current) =>
                _state.Value = new HealthState(current, current - _state.Value.Current);
        }
    }
}
