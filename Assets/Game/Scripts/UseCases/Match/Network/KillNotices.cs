using System;
using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.UseCases.Match.Network.Requests;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Match.Network
{
    public sealed class KillNotices
    {
        private readonly IClientPlayerCollection _clientPlayers;
        private readonly IReadonlyWeaponDescriptionRegistry _descriptions;

        public KillNotices(IClientPlayerCollection clientPlayers, IReadonlyWeaponDescriptionRegistry descriptions)
        {
            _clientPlayers = clientPlayers;
            _descriptions = descriptions;
        }

        public KillNotice Of(Kill kill) =>
            new(TagOf(kill.Killer), TagOf(kill.Victim), _descriptions.DescriptionOf(kill.Weapon).Key);

        private PlayerTag TagOf(IPlayer player)
        {
            if (!_clientPlayers.ByClient.TryGetLeft(player, out var client)) //TODO: боты — у игрока без клиента нет id, по которому клиент узнал бы в ленте себя
                throw new InvalidOperationException("The kill feed names only network players, and this player has no client.");

            return new PlayerTag(client.Id, player.Nickname);
        }
    }
}
