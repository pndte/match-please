using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.LagCompensation
{
    public interface IRewindableCollection : IViewableList<IRewindable>
    {
    }

    public sealed class RewindableCollection : ViewableList<IRewindable>, IRewindableCollection
    {
    }
}
