using System;
using Bw.Entities.Network;
using Bw.UseCases.Match.Network.Requests;

namespace Bw.UseCases.Match.Network
{
    public sealed class MatchTables
    {
        private readonly IReadonlyMatch _match;
        private readonly IReadonlyScoreboard _scoreboard;
        private readonly IClientPlayerCollection _clientPlayers;

        public MatchTables(IReadonlyMatch match, IReadonlyScoreboard scoreboard, IClientPlayerCollection clientPlayers)
        {
            _match = match;
            _scoreboard = scoreboard;
            _clientPlayers = clientPlayers;
        }

        public MatchTable Create()
        {
            var entries = new MatchEntry[_scoreboard.Scores.Count];
            var index = 0;
            foreach (var (player, score) in _scoreboard.Scores)
            {
                if (!_clientPlayers.ByClient.TryGetLeft(player, out var client)) //TODO: боты — у игрока без клиента нет id для строки «ты»
                    throw new InvalidOperationException("The match table lists only network players, and this player has no client.");

                entries[index++] = new MatchEntry(client.Id, player.Nickname, player.CharacterKey, score);
            }

            return new MatchTable(_match.Period.Value, entries);
        }
    }
}
