using System.Collections.Generic;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.Guarding.Extensions;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality.Guarding.Extensions
{
    public sealed class GuardCoverExtensionsTests
    {
        private readonly RightSideCover _cover = new();

        [Test]
        public void NothingCoveredLeavesEveryImpact()
        {
            var uncovered = _cover.Uncovered(Blows.Of(Blows.At(-1f, 0f, 10f), Blows.At(-2f, 0f, 20f)));

            Assert.That(DamagesOf(uncovered), Is.EqualTo(new[] { 10f, 20f }));
        }

        [Test]
        public void CoveredImpactsAreDroppedAndTheRestKeepTheirOrder()
        {
            var impacts = Blows.Of(Blows.At(-1f, 0f, 1f), Blows.At(1f, 0f, 2f), Blows.At(-1f, 1f, 3f), Blows.At(2f, 0f, 4f), Blows.At(-3f, 0f, 5f));

            var uncovered = _cover.Uncovered(impacts);

            Assert.That(DamagesOf(uncovered), Is.EqualTo(new[] { 1f, 3f, 5f }));
        }

        [Test]
        public void EverythingCoveredLeavesNoImpacts()
        {
            var uncovered = _cover.Uncovered(Blows.Of(Blows.At(1f, 0f, 10f), Blows.At(2f, 0f, 20f)));

            Assert.That(uncovered.Count, Is.Zero);
        }

        [Test]
        public void NoImpactsLeaveNoImpacts()
        {
            Assert.That(_cover.Uncovered(default).Count, Is.Zero);
        }

        [Test]
        public void CoveredDamageSumsOnlyTheCoveredImpacts()
        {
            var impacts = Blows.Of(Blows.At(-1f, 0f, 1f), Blows.At(1f, 0f, 2f), Blows.At(2f, 0f, 4f));

            Assert.That(_cover.CoveredDamage(impacts), Is.EqualTo(6f));
        }

        [Test]
        public void NothingCoveredIsNoCoveredDamage()
        {
            Assert.That(_cover.CoveredDamage(Blows.Of(Blows.At(-1f, 0f, 10f))), Is.Zero);
            Assert.That(_cover.CoveredDamage(default), Is.Zero);
        }

        private static List<float> DamagesOf(Impacts impacts)
        {
            var damages = new List<float>();
            foreach (var impact in impacts)
                damages.Add(impact.Hit.Damage);

            return damages;
        }

        private sealed class RightSideCover : IGuardCover
        {
            public bool Covers(Contact contact) =>
                contact.Point.x > 0f;
        }
    }
}
