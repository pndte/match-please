using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Ticks
{
    public interface IScheduledCollection : IViewableList<IScheduled>
    {
    }

    public sealed class ScheduledCollection : ViewableList<IScheduled>, IScheduledCollection
    {
    }
}
