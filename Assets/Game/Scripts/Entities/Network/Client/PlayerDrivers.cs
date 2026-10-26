using System;
using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Entities.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network
{
    public sealed class PlayerDrivers : IPlayerDrivers //TODO: ведущий игрока не меняется, пока игрок в сессии: переподключение и замена ушедшего ботом должны менять значение Of, а не впускать игрока заново
    {
        public IViewableMap<IClient, IPlayer> ByClient => _byClient;

        private readonly Dictionary<IPlayer, ViewableProperty<PlayerDriver>> _drivers = new();
        private readonly ViewableMap<IClient, IPlayer> _byClient = new();

        public IReadonlyProperty<PlayerDriver> Of(IPlayer player)
        {
            if (!_drivers.TryGetValue(player, out var driver))
                throw new InvalidOperationException($"Player '{player.Nickname}' has no driver: whoever brings a player into the session names who drives it.");

            return driver;
        }

        public IPlayer PlayerOf(IClient client)
        {
            if (!_byClient.TryGetValue(client, out var player))
                throw new InvalidOperationException($"Client '{client}' drives no player: a client gets its player when it connects.");

            return player;
        }

        public void AddClientDriven(Lifetime lifetime, IPlayer player, IClient client)
        {
            if (_byClient.TryGetValue(client, out var driven))
                throw new InvalidOperationException($"Client '{client}' drives one player at a time: it already drives '{driven.Nickname}'.");

            Add(lifetime, player, PlayerDriver.Client(client));
            _byClient.AddLifetimed(lifetime, client, player);
        }

        public void AddServerDriven(Lifetime lifetime, IPlayer player) =>
            Add(lifetime, player, PlayerDriver.Server);

        private void Add(Lifetime lifetime, IPlayer player, PlayerDriver driver)
        {
            if (!_drivers.TryAdd(player, new ViewableProperty<PlayerDriver>(driver)))
                throw new InvalidOperationException($"Player '{player.Nickname}' has one driver at a time: it was added again before its last driver left.");

            lifetime.OnTermination(() => _drivers.Remove(player));
        }
    }
}
