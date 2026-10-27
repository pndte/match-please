using Bw.Entities;
using Bw.Entities.Players;

namespace Bw.UseCases.Bots.Network
{
    public interface IReadonlyBotRoster
    {
        public IReadonlyViewableMap<IPlayer, BotEntry> Bots { get; }
    }

    public interface IBotRoster : IReadonlyBotRoster
    {
        public void Add(string nickname, BotPreset preset);
        public void Remove(IPlayer bot);
        public void Change(IPlayer bot, BotPreset preset);
    }
}
