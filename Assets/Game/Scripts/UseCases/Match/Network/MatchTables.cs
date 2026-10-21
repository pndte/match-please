using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.UseCases.Match.Network.Extensions;
using Bw.UseCases.Match.Network.Requests;

namespace Bw.UseCases.Match.Network
{
    public sealed class MatchTables
    {
        private readonly IReadonlyMatch _match;
        private readonly IReadonlyScoreboard _scoreboard;
        private readonly IReadonlyPlayerIds _ids;
        private readonly IReadonlyPlayerCharacters _characters;

        public MatchTables(
            IReadonlyMatch match,
            IReadonlyScoreboard scoreboard,
            IReadonlyPlayerIds ids,
            IReadonlyPlayerCharacters characters)
        {
            _match = match;
            _scoreboard = scoreboard;
            _ids = ids;
            _characters = characters;
        }

        public MatchTable Create()
        {
            var entries = new MatchEntry[_scoreboard.Scores.Count];
            var index = 0;
            foreach (var (player, score) in _scoreboard.Scores)
                entries[index++] = new MatchEntry(_ids.TagOf(player), _characters.KeyOf(player), score);

            return new MatchTable(_match.Period.Value, entries);
        }
    }
}
