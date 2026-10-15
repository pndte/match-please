using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities
{
    public interface IControlledBy
    {
        public IReadonlyProperty<bool> Me { get; }
        public void SetMe(Lifetime lifetime);
    }
}
