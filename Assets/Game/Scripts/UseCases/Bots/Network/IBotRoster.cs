using Bw.Entities.Players;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Bots.Network
{
    public interface IBotRoster
    {
        public IViewableMap<IPlayer, BotEntry> Bots { get; }
    }
}
