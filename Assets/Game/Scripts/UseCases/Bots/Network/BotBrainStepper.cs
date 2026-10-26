using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Bots.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Bots.Network
{
    public sealed class BotBrainStepper
    {
        public BotBrainStepper(Lifetime lifetime, IBotBrain brain, IWorldStep worldStep)
        {
            worldStep.Stepped(TickPhase.Prepare).Advise(lifetime, _ => brain.Think());
        }
    }
}
