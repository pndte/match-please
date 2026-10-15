using System;
using Bw.CoreAdapters.Characters;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Match.Network.Requests;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.UseCases.Match.Network
{
    public sealed class MatchReceiver
    {
        private readonly NetworkObject _networkObject;
        private readonly IMatchBoard _board;
        private readonly CharacterCatalog _characters;

        public MatchReceiver(
            Lifetime lifetime,
            NetworkObject networkObject,
            INetEntries entries,
            NetPropertyDeclaration<MatchTable> declaration,
            IMatchBoard board,
            CharacterCatalog characters)
        {
            _networkObject = networkObject;
            _board = board;
            _characters = characters;

            entries.Get(declaration).Advise(lifetime, Apply);
        }

        private void Apply(MatchTable table)
        {
            var lines = table.Entries.Count == 0 ? Array.Empty<Standing>() : new Standing[table.Entries.Count];
            for (var index = 0; index < lines.Length; index++)
            {
                var entry = table.Entries[index];
                lines[index] = new Standing(entry.Nickname, _characters.CharacterOf(entry.CharacterKey), entry.Score, entry.ClientId == _networkObject.NetworkManager.LocalClientId);
            }

            Array.Sort(lines, StandingOrder.Instance);
            _board.Apply(new MatchStandings(table.Period, lines));
        }
    }
}
