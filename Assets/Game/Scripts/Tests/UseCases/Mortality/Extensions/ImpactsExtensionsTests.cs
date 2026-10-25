using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Mortality.Extensions
{
    public sealed class ImpactsExtensionsTests
    {
        [Test]
        public void DamageIsTheSumOfEveryImpact()
        {
            var impacts = Blows.Of(Blows.At(0f, 0f, 12.5f), Blows.At(1f, 0f, 7.5f), Blows.At(0f, 1f, 5f));

            Assert.That(impacts.Damage(), Is.EqualTo(25f));
        }

        [Test]
        public void KnockbackIsTheSumOfEveryImpact()
        {
            var impacts = Blows.Of(
                Blows.Pushing(new Vector2(2f, 1f), Vector2.zero),
                Blows.Pushing(new Vector2(-0.5f, 3f), Vector2.one));

            Assert.That(impacts.Knockback(), Is.EqualTo(new Vector2(1.5f, 4f)));
        }

        [Test]
        public void EmptyImpactsDealNothing()
        {
            var impacts = default(Impacts);

            Assert.That(impacts.Damage(), Is.Zero);
            Assert.That(impacts.Knockback(), Is.EqualTo(Vector2.zero));
        }
    }
}
