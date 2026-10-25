using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using NUnit.Framework;

namespace Bw.Tests.Entities.Extensions
{
    public sealed class HealthStateExtensionsTests
    {
        [TestCase(100f, 30f, 70f, -30f)]
        [TestCase(100f, 100f, 0f, -100f)]
        [TestCase(20f, 50f, 0f, -20f)]
        [TestCase(0f, 10f, 0f, 0f)]
        [TestCase(50f, 0.25f, 49.75f, -0.25f)]
        public void HitTakesDamageDownToZero(
            float current,
            float damage,
            float expectedCurrent,
            float expectedChange)
        {
            var after = new HealthState(current, 5f).AfterHit(damage);

            Assert.That(after.Current, Is.EqualTo(expectedCurrent).Within(1e-5f));
            Assert.That(after.Change, Is.EqualTo(expectedChange).Within(1e-5f));
        }

        [TestCase(0f)]
        [TestCase(-5f)]
        public void HitWithoutDamageIsRefused(float damage)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HealthState(50f, 0f).AfterHit(damage));
        }

        [TestCase(40f, 30f, 100f, 70f, 30f)]
        [TestCase(90f, 30f, 100f, 100f, 10f)]
        [TestCase(100f, 5f, 100f, 100f, 0f)]
        [TestCase(0f, 100f, 100f, 100f, 100f)]
        public void HealRestoresUpToTheMax(
            float current,
            float amount,
            float max,
            float expectedCurrent,
            float expectedChange)
        {
            var after = new HealthState(current, -5f).AfterHeal(amount, max);

            Assert.That(after.Current, Is.EqualTo(expectedCurrent).Within(1e-5f));
            Assert.That(after.Change, Is.EqualTo(expectedChange).Within(1e-5f));
        }

        [TestCase(0f)]
        [TestCase(-5f)]
        public void HealWithoutAnAmountIsRefused(float amount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HealthState(50f, 0f).AfterHeal(amount, 100f));
        }

        [Test]
        public void WithoutChangeKeepsTheHealthAndDropsTheChange()
        {
            var after = new HealthState(42f, -8f).WithoutChange();

            Assert.That(after.Current, Is.EqualTo(42f));
            Assert.That(after.Change, Is.EqualTo(0f));
        }
    }
}
