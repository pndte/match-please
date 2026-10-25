using System;
using System.Linq;
using Bw.UseCases.Match;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Match
{
    public sealed class StandingOrderTests
    {
        [Test]
        public void MoreKillsRankFirst()
        {
            Assert.That(Ranked(Line("a", 1, 0), Line("b", 3, 9)), Is.EqualTo(new[] { "b", "a" }));
        }

        [Test]
        public void EqualKillsRankByKillsPerDeath()
        {
            Assert.That(Ranked(Line("a", 2, 2), Line("b", 2, 1)), Is.EqualTo(new[] { "b", "a" }));
        }

        [Test]
        public void NoDeathsTieWithOneDeathAndFewerDeathsWin()
        {
            Assert.That(Ranked(Line("a", 2, 1), Line("b", 2, 0)), Is.EqualTo(new[] { "b", "a" }));
        }

        [Test]
        public void EqualRatiosRankByFewerDeaths()
        {
            Assert.That(Ranked(Line("a", 0, 3), Line("b", 0, 1)), Is.EqualTo(new[] { "b", "a" }));
        }

        [Test]
        public void FullTiesRankByNicknameOrdinally()
        {
            Assert.That(Ranked(Line("b", 1, 1), Line("B", 1, 1), Line("a", 1, 1)), Is.EqualTo(new[] { "B", "a", "b" }));
        }

        [Test]
        public void DamageBreaksNoTies()
        {
            Assert.That(Ranked(Line("b", 1, 1, 500), Line("a", 1, 1, 10)), Is.EqualTo(new[] { "a", "b" }));
        }

        [Test]
        public void StandingIsEqualToItself()
        {
            var line = Line("a", 1, 1);

            Assert.That(StandingOrder.Instance.Compare(line, line), Is.Zero);
        }

        private static string[] Ranked(params Standing[] lines)
        {
            Array.Sort(lines, StandingOrder.Instance);
            return lines.Select(line => line.Nickname).ToArray();
        }

        private static Standing Line(
            string nickname,
            int kills,
            int deaths,
            int damage = 0) =>
            new(nickname, null, new Score(kills, deaths, damage), false);
    }
}
