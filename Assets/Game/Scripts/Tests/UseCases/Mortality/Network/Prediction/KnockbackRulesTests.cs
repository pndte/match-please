using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Network.Prediction;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Mortality.Network.Prediction
{
    public sealed class KnockbackRulesTests
    {
        [Test]
        public void ImpulseIsTheSummedKnockbackOfEveryImpact()
        {
            var impacts = Blows.Of(Blows.Pushing(new Vector2(4f, 1f), Vector2.zero), Blows.Pushing(new Vector2(-1f, 2f), Vector2.up));

            Assert.That(new KnockbackRules().Impulse(impacts), Is.EqualTo(new Vector2(3f, 3f)));
        }

        [Test]
        public void NoImpactsPushNothing()
        {
            Assert.That(new KnockbackRules().Impulse(default(Impacts)), Is.EqualTo(Vector2.zero));
        }
    }
}
