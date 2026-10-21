using Bw.Entities.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network
{
    public sealed class PlayerEntrance : IPlayerEntrance
    {
        private readonly IPlayerIds _ids;
        private readonly IPlayerDrivers _drivers;
        private readonly IPlayerCharacters _characters;
        private readonly IPlayerCollection _players;

        public PlayerEntrance(
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
            _ids.AddLifetimed(lifetime, player);
            driver.Switch(
                client => _drivers.AddClientDriven(lifetime, player, client),
                () => _drivers.AddServerDriven(lifetime, player));
            _characters.AddLifetimed(lifetime, player, characterKey);
            _players.AddLifetimed(lifetime, player);
        }
    }
}
