using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities
{
    public interface IReadonlyControlledBy
    {
        public IReadonlyProperty<bool> Me { get; }
    }

    public interface IControlledBy : IReadonlyControlledBy
    {
        public void SetMe(Lifetime lifetime);
    }
}
