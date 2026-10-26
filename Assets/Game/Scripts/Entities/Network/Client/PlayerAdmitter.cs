using System;
using Bw.Entities.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network
{
    public sealed class PlayerAdmitter : IPlayerAdmitter
    {
        private readonly IPlayerIds _ids;
        private readonly IPlayerDrivers _drivers;
        private readonly IPlayerCharacters _characters;
        private readonly IPlayerCollection _players;

        public PlayerAdmitter(
            IPlayerIds ids,
            IPlayerDrivers drivers,
            IPlayerCharacters characters,
            IPlayerCollection players)
        {
            _ids = ids;
            _drivers = drivers;
            _characters = characters;
            _players = players;
        }

        public void Admit(
            Lifetime lifetime,
            IPlayer player,
            int characterKey,
            PlayerDriver driver)
        {
            if (_players.Contains(player))
                throw new InvalidOperationException($"Player '{player.Nickname}' is admitted once: it is already in the session.");

            if (driver.Switch(_drivers, static (drivers, client) => drivers.ByClient.ContainsKey(client), static _ => false))
                throw new InvalidOperationException($"Player '{player.Nickname}' cannot be driven by a client that already drives another player.");

            _ids.AddLifetimed(lifetime, player);
            driver.Switch(
                client => _drivers.AddClientDriven(lifetime, player, client),
                () => _drivers.AddServerDriven(lifetime, player));
            _characters.AddLifetimed(lifetime, player, characterKey);
            _players.AddLifetimed(lifetime, player);
        }
    }
}
