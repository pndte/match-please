using Bw.Entities;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Network.Prediction;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality.Network.Prediction
{
    public sealed class VitalsHealRulesTests
    {
        private MortalFixture _fixture;
        private VitalsHealRules _rules;

        [SetUp]
        public void SetUp()
        {
            _fixture = new MortalFixture(80f);
            _rules = new VitalsHealRules(_fixture.Health);
        }

        [TearDown]
        public void TearDown() =>
            _fixture.Dispose();

        [Test]
        public void HealRestoresTheLivingAndShowsItsGain()
        {
            var vitals = _rules.Apply(VitalsOf.Alive(50f), new Heal(10f));

            Assert.That(vitals.Health, Is.EqualTo(new HealthState(60f, 10f)));
            Assert.That(vitals.State, Is.EqualTo(LifeState.Alive));
        }

        [Test]
        public void HealStopsAtTheMaximumOfTheHealth()
        {
            var vitals = _rules.Apply(VitalsOf.Alive(75f), new Heal(10f));

            Assert.That(vitals.Health, Is.EqualTo(new HealthState(80f, 5f)));
        }

        [Test]
        public void HealOnTheDeadChangesNothing()
        {
            var vitals = _rules.Apply(VitalsOf.Dead(0f, -5f), new Heal(10f));

            Assert.That(vitals.Health, Is.EqualTo(new HealthState(0f, 0f)));
            Assert.That(vitals.State, Is.EqualTo(LifeState.Dead));
        }
    }
}
