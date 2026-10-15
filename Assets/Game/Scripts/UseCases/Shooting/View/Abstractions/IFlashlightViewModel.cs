using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.View.Abstractions
{
    public interface IFlashlightViewModel
    {
        public IReadonlyProperty<bool> Lit { get; }
    }
}
