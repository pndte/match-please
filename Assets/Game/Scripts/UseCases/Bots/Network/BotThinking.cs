using Bw.Entities.Network.Ticks;
using Bw.UseCases.Bots.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Bots.Network
{
    public sealed class BotThinking
    {
        public BotThinking(Lifetime lifetime, IBotBrain brain, INetworkTicks ticks)
        {
            ticks.Ticked(TickPhase.Late).Advise(lifetime, _ => brain.Think());
        }
    }
}
