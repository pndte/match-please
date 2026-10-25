using System;
using System.Collections.Generic;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality
{
    public sealed class ImpactsTests
    {
        [Test]
        public void DefaultImpactsAreEmpty()
        {
            var impacts = default(Impacts);
            var enumerated = 0;
            foreach (var _ in impacts)
                enumerated++;

            Assert.That(impacts.Count, Is.Zero);
            Assert.That(enumerated, Is.Zero);
        }

        [Test]
        public void SingleImpactIsTheOnlyOne()
        {
            var impacts = new Impacts(Blows.At(1f, 2f, 30f));

            Assert.That(impacts.Count, Is.EqualTo(1));
            Assert.That(impacts[0].Hit.Damage, Is.EqualTo(30f));
            Assert.That(impacts[0].Contact.Point.x, Is.EqualTo(1f));
        }

        [Test]
        public void ImpactsKeepTheirOrder()
        {
            var impacts = Blows.Of(Blows.At(0f, 0f, 1f), Blows.At(0f, 0f, 2f), Blows.At(0f, 0f, 3f));
            var damages = new List<float>();
            foreach (var impact in impacts)
                damages.Add(impact.Hit.Damage);

            Assert.That(impacts.Count, Is.EqualTo(3));
            Assert.That(damages, Is.EqualTo(new[] { 1f, 2f, 3f }));
            Assert.That(impacts[2].Hit.Damage, Is.EqualTo(3f));
        }

        [Test]
        public void IndexPastTheLastImpactThrows()
        {
            var impacts = new Impacts(Blows.At(0f, 0f, 1f));

            Assert.Throws<IndexOutOfRangeException>(() => _ = impacts[1]);
        }
    }
}
