using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Bots.Network
{
    public sealed class BotRoster : IBotRoster
    {
        public IViewableMap<IPlayer, BotEntry> Bots => _bots;

        private readonly ViewableMap<IPlayer, BotEntry> _bots = new();
        private readonly Lifetime _lifetime;
        private readonly IPlayerEntrance _entrance;
        private readonly BotsConfig _config;

        public BotRoster(
            Lifetime lifetime,
            INetworkHolder networkHolder,
            IPlayerEntrance entrance,
            BotsConfig config)
        {
            _lifetime = lifetime;
            _entrance = entrance;
            _config = config;

            networkHolder.NetworkManager.AdviseNotNull(lifetime, _ => Join());
        }

        private void Join()
        {
            foreach (var entry in _config.Bots)
            {
                var player = new Player(entry.Nickname);
                _bots.AddLifetimed(_lifetime, player, entry);
                _entrance.Admit(_lifetime, player, entry.Character.Key, PlayerDriver.Server);
            }
        }
    }
}
