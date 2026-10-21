using System;
using System.Collections.Generic;

namespace Bw.UseCases.Match.Network.Requests
{
    public sealed class MatchTable
    {
        public static readonly MatchTable Empty = new(MatchPeriod.Starting, Array.Empty<MatchEntry>());

        public readonly MatchPeriod Period;
        public readonly IReadOnlyList<MatchEntry> Entries;

        public MatchTable(MatchPeriod period, IReadOnlyList<MatchEntry> entries)
        {
            Period = period;
            Entries = entries;
        }
    }

    public readonly struct MatchEntry
    {
        public readonly PlayerTag Player;
        public readonly int CharacterKey;
        public readonly Score Score;

        public MatchEntry(PlayerTag player, int characterKey, Score score)
        {
            Player = player;
            CharacterKey = characterKey;
            Score = score;
        }
    }
}
