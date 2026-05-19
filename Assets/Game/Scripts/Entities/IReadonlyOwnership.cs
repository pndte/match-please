using JetBrains.Collections.Viewable;

namespace Bw.Entities
{
    public interface IReadonlyOwnership
    {
        public IReadonlyProperty<bool> Mine { get; }
    }
}