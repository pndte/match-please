using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Ticks
{
    public sealed class ScheduledRegistration
    {
        public ScheduledRegistration(Lifetime lifetime, IScheduled scheduled, IScheduledCollection collection)
        {
            collection.AddLifetimed(lifetime, scheduled);
        }
    }
}
