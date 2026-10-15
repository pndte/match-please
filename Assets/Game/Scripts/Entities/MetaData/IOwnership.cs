using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities
{
    public interface IOwnership
    {
        public IReadonlyProperty<bool> Mine { get; }
        public void SetMine(Lifetime lifetime);
    }
}