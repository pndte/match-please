using System;
using System.Collections.Generic;
using Bw.UseCases.Match.Extensions;

namespace Bw.UseCases.Match
{
    public sealed class StandingOrder : IComparer<Standing>
    {
        public static readonly StandingOrder Instance = new();

        public int Compare(Standing left, Standing right)
        {
            var kills = right.Score.Kills.CompareTo(left.Score.Kills);
            if (kills != 0)
                return kills;

            var killsPerDeath = right.Score.KillsPerDeath().CompareTo(left.Score.KillsPerDeath());
            if (killsPerDeath != 0)
                return killsPerDeath;

            var deaths = left.Score.Deaths.CompareTo(right.Score.Deaths);
            return deaths != 0 ? deaths : string.Compare(left.Nickname, right.Nickname, StringComparison.Ordinal);
        }
    }
}
