using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Level.View.Abstractions
{
    public interface ILampLightViewModel
    {
        public IReadonlyProperty<bool> Live { get; }
    }
}
