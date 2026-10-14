using System;
using System.Collections.Generic;

namespace Bw.UseCases.Match
{
    public sealed class MatchStandings
    {
        public static readonly MatchStandings None = new(MatchPeriod.Starting, Array.Empty<Standing>());

        public readonly MatchPeriod Period;
        public readonly IReadOnlyList<Standing> Lines;

        public MatchStandings(MatchPeriod period, IReadOnlyList<Standing> lines)
        {
            Period = period;
            Lines = lines;
        }
    }
}
