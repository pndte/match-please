using System;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Replication
{
    public sealed class TickSteps : ISource<Unit>
    {
        private readonly INetworkTicks _ticks;

        public TickSteps(INetworkTicks ticks)
        {
            _ticks = ticks;
        }

        public void Advise(Lifetime lifetime, Action<Unit> handler) =>
            _ticks.Ticked(TickPhase.Default).Advise(lifetime, _ => handler(Unit.Instance));
    }
}
