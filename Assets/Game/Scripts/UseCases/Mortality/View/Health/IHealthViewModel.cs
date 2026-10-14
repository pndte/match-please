using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Mortality.View.Health
{
    public interface IHealthViewModel
    {
        public float Max { get; }
        public IReadonlyProperty<HealthLevel> Level { get; }
    }
}
