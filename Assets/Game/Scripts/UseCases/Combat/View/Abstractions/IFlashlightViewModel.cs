using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.View.Abstractions
{
    public interface IFlashlightViewModel
    {
        public IReadonlyProperty<bool> Lit { get; }
    }
}
