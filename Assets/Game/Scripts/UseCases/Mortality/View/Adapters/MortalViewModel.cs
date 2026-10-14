using Bw.Entities.Extensions;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Mortality.View.Death;
using Bw.UseCases.Mortality.View.Health;
using Bw.UseCases.Mortality.View.Hit;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Mortality.View.Adapters
{
    public sealed class MortalViewModel : IHealthViewModel, IDamageViewModel, IDeathViewModel
    {
        public float Max => _mortal.Health.Max;
        public IReadonlyProperty<HealthLevel> Level { get; }
        public ISource<Unit> Taken => _taken;
        public IReadonlyProperty<bool> Dead { get; }
        public ISource<Unit> Killed => _killed;

        private readonly IReadonlyMortal _mortal;
        private readonly Signal<Unit> _taken = new();
        private readonly Signal<Unit> _killed = new();

        public MortalViewModel(Lifetime lifetime, IReadonlyMortal mortal)
        {
            _mortal = mortal;
            Level = mortal.Health.State.Select(lifetime, state => new HealthLevel(state.Current / mortal.Health.Max, state.Change / mortal.Health.Max));
            Dead = mortal.State.Select(lifetime, state => state == LifeState.Dead);

            mortal.Health.AdviseDamage(lifetime, _ => _taken.Fire(Unit.Instance));
            mortal.AdviseKilled(lifetime, () => _killed.Fire(Unit.Instance));
        }
    }
}
