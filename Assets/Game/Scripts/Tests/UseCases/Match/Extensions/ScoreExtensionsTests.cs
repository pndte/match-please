using Bw.UseCases.Match;
using Bw.UseCases.Match.Extensions;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Match.Extensions
{
    public sealed class ScoreExtensionsTests
    {
        [Test]
        public void PlusAddsEveryCount()
        {
            var sum = new Score(2, 3, 40).Plus(new Score(1, 0, 15));

            Assert.That(sum.Kills, Is.EqualTo(3));
            Assert.That(sum.Deaths, Is.EqualTo(3));
            Assert.That(sum.Damage, Is.EqualTo(55));
        }

        [Test]
        public void PlusNothingKeepsTheScore()
        {
            var score = new Score(4, 1, 7).Plus(default);

            Assert.That(score.Kills, Is.EqualTo(4));
            Assert.That(score.Deaths, Is.EqualTo(1));
            Assert.That(score.Damage, Is.EqualTo(7));
        }

        [TestCase(0, 0, 0f)]
        [TestCase(3, 0, 3f)]
        [TestCase(3, 1, 3f)]
        [TestCase(3, 2, 1.5f)]
        [TestCase(1, 4, 0.25f)]
        [TestCase(0, 5, 0f)]
        public void KillsPerDeathCountsNoDeathsAsOne(int kills, int deaths, float ratio)
        {
            Assert.That(new Score(kills, deaths, 0).KillsPerDeath(), Is.EqualTo(ratio).Within(1e-6f));
        }

        [TestCase(1, 100, 100)]
        [TestCase(3, 100, 33)]
        [TestCase(3, 11, 4)]
        [TestCase(4, 1, 0)]
        public void DamagePerKillIsRoundedToAWholeNumber(int kills, int damage, int perKill)
        {
            Assert.That(new Score(kills, 0, damage).TryGetDamagePerKill(out var damagePerKill), Is.True);
            Assert.That(damagePerKill, Is.EqualTo(perKill));
        }

        [Test]
        public void NoKillsHaveNoDamagePerKill()
        {
            Assert.That(new Score(0, 2, 250).TryGetDamagePerKill(out var damagePerKill), Is.False);
            Assert.That(damagePerKill, Is.Zero);
        }
    }
}
