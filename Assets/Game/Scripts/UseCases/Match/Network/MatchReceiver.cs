using System;
using Bw.CoreAdapters.Characters;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Match.Network.Requests;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Match.Network
{
    public sealed class MatchReceiver
    {
        private readonly IMatchBoard _board;
        private readonly CharacterCatalog _characters;

        public MatchReceiver(
            Lifetime lifetime,
            INetEntries entries,
            NetPropertyDeclaration<MatchTable> declaration,
            IMatchBoard board,
            CharacterCatalog characters,
            ILocalPlayer localPlayer)
        {
            _board = board;
            _characters = characters;

            var table = entries.Property(declaration);
            localPlayer.Id.View(lifetime, (idLifetime, id) =>
                table.Advise(idLifetime, received => Apply(received, id)));
        }

        private void Apply(MatchTable table, LocalPlayerId localPlayer)
        {
            var lines = table.Entries.Count == 0 ? Array.Empty<Standing>() : new Standing[table.Entries.Count];
            for (var index = 0; index < lines.Length; index++)
            {
                var entry = table.Entries[index];
                lines[index] = new Standing(entry.Player.Nickname, _characters.CharacterOf(entry.CharacterKey), entry.Score, localPlayer.Is(entry.Player.Id));
            }

            Array.Sort(lines, StandingOrder.Instance);
            _board.Apply(new MatchStandings(table.Period, lines));
        }
    }
}
