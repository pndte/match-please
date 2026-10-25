using System;
using Bw.Entities;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality.Extensions
{
    public sealed class VitalsExtensionsTests
    {
        [TestCase(100f, 30f, 70f, -30f)]
        [TestCase(30f, 29.5f, 0.5f, -29.5f)]
        [TestCase(0.25f, 0.125f, 0.125f, -0.125f)]
        public void HitThatLeavesHealthTakesTheDamage(
            float health,
            float damage,
            float left,
            float change)
        {
            var after = VitalsOf.Alive(health).AfterHit(damage);

            Assert.That(after.Health, Is.EqualTo(new HealthState(left, change)));
            Assert.That(after.State, Is.EqualTo(LifeState.Alive));
        }

        [TestCase(30f, 30f, -30f)]
        [TestCase(20f, 50f, -20f)]
        [TestCase(0.5f, 1000f, -0.5f)]
        public void HitDownToZeroKillsAndChangesOnlyByTheRealLoss(float health, float damage, float change)
        {
            var after = VitalsOf.Alive(health).AfterHit(damage);

            Assert.That(after.Health, Is.EqualTo(new HealthState(0f, change)));
            Assert.That(after.State, Is.EqualTo(LifeState.Dead));
        }

        [Test]
        public void HitOnALivingMortalAtZeroHealthKillsWithoutChange()
        {
            var after = VitalsOf.Alive(0f).AfterHit(10f);

            Assert.That(after.Health, Is.EqualTo(new HealthState(0f, 0f)));
            Assert.That(after.State, Is.EqualTo(LifeState.Dead));
        }

        [Test]
        public void HitOnTheDeadThrows()
        {
            Assert.Throws<InvalidOperationException>(() => VitalsOf.Dead().AfterHit(10f));
        }

        [TestCase(0f)]
        [TestCase(-5f)]
        public void HitWithoutDamageThrows(float damage)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => VitalsOf.Alive(50f).AfterHit(damage));
        }

        [TestCase(50f, 10f, 100f, 60f, 10f)]
        [TestCase(95f, 10f, 100f, 100f, 5f)]
        [TestCase(100f, 10f, 100f, 100f, 0f)]
        [TestCase(0f, 8f, 100f, 8f, 8f)]
        public void HealRestoresUpToTheMaximumAndChangesByTheRealGain(
            float health,
            float amount,
            float max,
            float healed,
            float change)
        {
            var after = VitalsOf.Alive(health, -3f).AfterHeal(amount, max);

            Assert.That(after.Health, Is.EqualTo(new HealthState(healed, change)));
            Assert.That(after.State, Is.EqualTo(LifeState.Alive));
        }

        [Test]
        public void HealOnTheDeadThrows()
        {
            Assert.Throws<InvalidOperationException>(() => VitalsOf.Dead().AfterHeal(10f, 100f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void HealWithoutAmountThrows(float amount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => VitalsOf.Alive(50f).AfterHeal(amount, 100f));
        }

        [Test]
        public void WithoutChangeKeepsHealthAndLifeState()
        {
            var vitals = VitalsOf.Dead(0f, -40f).WithoutChange();

            Assert.That(vitals.Health, Is.EqualTo(new HealthState(0f, 0f)));
            Assert.That(vitals.State, Is.EqualTo(LifeState.Dead));
        }

        [Test]
        public void LethalHitAfterLifeIsAKill()
        {
            Assert.That(VitalsOf.Dead(0f, -30f).IsKillAfter(VitalsOf.Alive(30f)), Is.True);
        }

        [Test]
        public void DeathSnapshotOfALivingMortalIsNoKill()
        {
            Assert.That(VitalsOf.Dead().IsKillAfter(VitalsOf.Alive(100f)), Is.False);
        }

        [Test]
        public void ConfirmedDeathAfterAPredictedLethalHitIsAKill()
        {
            Assert.That(VitalsOf.Dead().IsKillAfter(VitalsOf.Alive(0f, -30f)), Is.True);
        }

        [Test]
        public void DeathAfterDeathIsNoKill()
        {
            Assert.That(VitalsOf.Dead(0f, -10f).IsKillAfter(VitalsOf.Dead()), Is.False);
        }

        [Test]
        public void SurvivedHitIsNoKill()
        {
            Assert.That(VitalsOf.Alive(10f, -20f).IsKillAfter(VitalsOf.Alive(30f)), Is.False);
        }

        [Test]
        public void PredictedLethalHitWithoutDeathIsNoKillYet()
        {
            Assert.That(VitalsOf.Alive(0f, -30f).IsKillAfter(VitalsOf.Alive(30f)), Is.False);
        }
    }
}
