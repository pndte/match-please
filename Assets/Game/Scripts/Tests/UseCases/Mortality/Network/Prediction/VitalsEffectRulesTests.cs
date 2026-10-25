using Bw.Entities;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Network.Prediction;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality.Network.Prediction
{
    public sealed class VitalsEffectRulesTests
    {
        private readonly VitalsEffectRules _rules = new();

        [Test]
        public void ImpactsCostTheLivingTheirSummedDamage()
        {
            var vitals = _rules.Apply(VitalsOf.Alive(100f), Blows.Of(Blows.At(0f, 0f, 12f), Blows.At(1f, 0f, 12f), Blows.At(0f, 1f, 6f)));

            Assert.That(vitals.Health, Is.EqualTo(new HealthState(70f, -30f)));
            Assert.That(vitals.State, Is.EqualTo(LifeState.Alive));
        }

        [Test]
        public void LethalImpactsKill()
        {
            var vitals = _rules.Apply(VitalsOf.Alive(20f), Blows.Of(Blows.At(0f, 0f, 30f)));

            Assert.That(vitals.Health, Is.EqualTo(new HealthState(0f, -20f)));
            Assert.That(vitals.State, Is.EqualTo(LifeState.Dead));
        }

        [Test]
        public void ImpactsOnTheDeadChangeNothing()
        {
            var vitals = _rules.Apply(VitalsOf.Dead(0f, -20f), Blows.Of(Blows.At(0f, 0f, 30f)));

            Assert.That(vitals.Health, Is.EqualTo(new HealthState(0f, 0f)));
            Assert.That(vitals.State, Is.EqualTo(LifeState.Dead));
        }
    }
}
