using Bw.Entities;
using Bw.Entities.Network.Prediction.Events;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Network.Prediction;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality.Network.Prediction
{
    public sealed class VitalsPredictionRulesTests
    {
        private readonly VitalsPredictionRules _rules = new();

        [Test]
        public void PredictedImpactsCostTheirSummedDamage()
        {
            var vitals = _rules.Predict(VitalsOf.Alive(100f), Blows.Of(Blows.At(0f, 0f, 20f), Blows.At(0f, 0f, 5f)));

            Assert.That(vitals.Health, Is.EqualTo(new HealthState(75f, -25f)));
        }

        [Test]
        public void PredictionNeverKills()
        {
            var vitals = _rules.Predict(VitalsOf.Alive(10f), Blows.Of(Blows.At(0f, 0f, 30f)));

            Assert.That(vitals.Health, Is.EqualTo(new HealthState(0f, -10f)));
            Assert.That(vitals.State, Is.EqualTo(LifeState.Alive));
        }

        [TestCase(80f, 60f, -20f)]
        [TestCase(60f, 80f, 0f)]
        [TestCase(70f, 70f, 0f)]
        public void PredictedChangeShowsOnlyALoss(float shown, float next, float change)
        {
            var presented = _rules.Present(VitalsOf.Alive(shown), VitalsOf.Alive(next), ChangeCause<Vitals, Impacts>.Predicted(Blows.Of(Blows.At(0f, 0f, 20f))));

            Assert.That(presented.Health, Is.EqualTo(new HealthState(next, change)));
        }

        [TestCase(80f, 60f, -20f, -20f)]
        [TestCase(60f, 80f, -20f, 0f)]
        [TestCase(80f, 90f, 10f, 10f)]
        [TestCase(90f, 80f, 10f, 0f)]
        [TestCase(80f, 60f, 0f, 0f)]
        [TestCase(60f, 80f, 0f, 0f)]
        [TestCase(70f, 50f, -30f, -20f)]
        public void AuthoritativeChangeShowsOnlyTheShiftTowardsItsOwnChange(
            float shown,
            float next,
            float serverChange,
            float change)
        {
            var server = VitalsOf.Alive(next, serverChange);

            var presented = _rules.Present(VitalsOf.Alive(shown), server, ChangeCause<Vitals, Impacts>.Authoritative(server));

            Assert.That(presented.Health, Is.EqualTo(new HealthState(next, change)));
        }

        [TestCase(80f, 60f)]
        [TestCase(60f, 80f)]
        public void CorrectionIsShownSilently(float shown, float next)
        {
            var presented = _rules.Present(VitalsOf.Alive(shown), VitalsOf.Alive(next, -40f), ChangeCause<Vitals, Impacts>.Correction);

            Assert.That(presented.Health, Is.EqualTo(new HealthState(next, 0f)));
        }

        [Test]
        public void PresentedStateIsTheNextLifeState()
        {
            var server = VitalsOf.Dead(0f, -30f);

            var presented = _rules.Present(VitalsOf.Alive(0f, -30f), server, ChangeCause<Vitals, Impacts>.Authoritative(server));

            Assert.That(presented.State, Is.EqualTo(LifeState.Dead));
            Assert.That(presented.Health, Is.EqualTo(new HealthState(0f, 0f)));
        }
    }
}
