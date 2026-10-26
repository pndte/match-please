using Bw.Entities.Extensions;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.View.Guarding;
using Bw.UseCases.Mortality.View.Health;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Mortality.View.Adapters
{
    public sealed class GuardViewModel : IGuardViewModel
    {
        public float Max => _guard.Max;
        public IReadonlyProperty<HealthLevel> Level => _level;
        public IReadonlyProperty<bool> Up { get; }

        private readonly IReadonlyGuard _guard;
        private readonly ViewableProperty<HealthLevel> _level;

        private float _health;

        public GuardViewModel(Lifetime lifetime, IReadonlyGuard guard)
        {
            _guard = guard;
            _health = guard.State.Value.Health;
            _level = new ViewableProperty<HealthLevel>(new HealthLevel(_health / guard.Max, 0f));
            Up = guard.State.Select(lifetime, state => state.Up);

            guard.State.Select(lifetime, state => state.Health).Advise(lifetime, Show);
        }

        private void Show(float health)
        {
            _level.Value = new HealthLevel(health / _guard.Max, (health - _health) / _guard.Max);
            _health = health;
        }
    }
}
