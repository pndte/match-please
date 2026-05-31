using JetBrains.Collections.Viewable;

namespace Bw.Entities
{
    public interface IOwnership
    {
        public IReadonlyProperty<bool> Mine { get; }
    }
}