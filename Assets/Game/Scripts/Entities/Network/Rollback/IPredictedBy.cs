using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Rollback
{
    public interface IPredictedBy
    {
        public IReadonlyProperty<bool> Me { get; }
    }
}
