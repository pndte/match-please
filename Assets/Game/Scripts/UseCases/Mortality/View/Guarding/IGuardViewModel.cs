using Bw.UseCases.Mortality.View.Health;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Mortality.View.Guarding
{
    public interface IGuardViewModel : IHealthViewModel
    {
        public IReadonlyProperty<bool> Up { get; }
    }
}
