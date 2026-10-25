using System;
using Bw.UseCases.Match;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Match
{
    public sealed class MatchBoardTests
    {
        [Test]
        public void BoardStartsWithNoStandings()
        {
            Assert.That(new MatchBoard().Current.Value, Is.SameAs(MatchStandings.None));
            Assert.That(MatchStandings.None.Period, Is.EqualTo(MatchPeriod.Starting));
            Assert.That(MatchStandings.None.Lines, Is.Empty);
        }

        [Test]
        public void AppliedStandingsAreCurrent()
        {
            var board = new MatchBoard();
            var standings = new MatchStandings(new MatchPeriod(MatchPhase.Running, 10), Array.Empty<Standing>());

            board.Apply(standings);

            Assert.That(board.Current.Value, Is.SameAs(standings));
        }
    }
}
