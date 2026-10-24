using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Rollback
{
    public interface IWorldStep
    {
        public IReadonlyProperty<bool> Replaying { get; }

        public ISource<int> Stepped(TickPhase phase);
    }
}
