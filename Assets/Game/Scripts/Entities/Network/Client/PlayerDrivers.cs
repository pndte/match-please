using System;
using System.Collections.Generic;
using Bw.Entities.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network
{
    public sealed class PlayerDrivers : IPlayerDrivers
    {
        public IViewableMap<IClient, IPlayer> ByClient => _byClient;

        private readonly Dictionary<IPlayer, ViewableProperty<PlayerDriver>> _drivers = new();
        private readonly ViewableMap<IClient, IPlayer> _byClient = new();

        public IReadonlyProperty<PlayerDriver> Of(IPlayer player) =>
            DriverOf(player);

        public IPlayer PlayerOf(IClient client)
        {
            if (!_byClient.TryGetValue(client, out var player))
                throw new InvalidOperationException($"Client '{client}' drives no player: a client gets its player when it connects.");

            return player;
        }

        public void AddClientDriven(Lifetime lifetime, IPlayer player, IClient client) =>
            Add(lifetime, player, PlayerDriver.Client(client));

        public void AddServerDriven(Lifetime lifetime, IPlayer player) =>
            Add(lifetime, player, PlayerDriver.Server);

        public void ChangeDriver(IPlayer player, PlayerDriver driver)
        {
            var current = DriverOf(player);
            RequireFree(driver);
            Release(current.Value);
            current.Value = driver;
            Bind(player, driver);
        }

        private void Add(Lifetime lifetime, IPlayer player, PlayerDriver driver)
        {
            RequireFree(driver);
            if (!_drivers.TryAdd(player, new ViewableProperty<PlayerDriver>(driver)))
                throw new InvalidOperationException($"Player '{player.Nickname}' has one driver at a time: it was added again before its last driver left.");

            Bind(player, driver);
            lifetime.OnTermination(() =>
            {
                Release(_drivers[player].Value);
                _drivers.Remove(player);
            });
        }

        private ViewableProperty<PlayerDriver> DriverOf(IPlayer player) =>
            _drivers.TryGetValue(player, out var driver)
                ? driver
                : throw new InvalidOperationException($"Player '{player.Nickname}' has no driver: whoever brings a player into the session names who drives it.");

        private void RequireFree(PlayerDriver driver) =>
            driver.Switch(
                _byClient,
                static (byClient, client) =>
                {
                    if (byClient.TryGetValue(client, out var driven))
                        throw new InvalidOperationException($"Client '{client}' drives one player at a time: it already drives '{driven.Nickname}'.");
                },
                static _ => { });

        private void Bind(IPlayer player, PlayerDriver driver) =>
            driver.Switch(client => _byClient.Add(client, player), () => { });

        private void Release(PlayerDriver driver) =>
            driver.Switch(client => _byClient.Remove(client), () => { });
    }
}
