using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Ticks
{
    public interface IInputDelay
    {
        public IReadonlyProperty<int> Ticks { get; }
    }
}
