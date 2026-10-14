using System;

namespace Bw.UseCases.Match
{
    public readonly struct MatchPeriod : IEquatable<MatchPeriod>
    {
        public static readonly MatchPeriod Starting = new(MatchPhase.Starting, 0);

        public readonly MatchPhase Phase;
        public readonly int EndTick;

        public MatchPeriod(MatchPhase phase, int endTick)
        {
            Phase = phase;
            EndTick = endTick;
        }

        public bool Equals(MatchPeriod other) =>
            Phase == other.Phase && EndTick == other.EndTick;
    }
}
