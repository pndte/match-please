using Bw.Entities;
using NUnit.Framework;

namespace Bw.Tests.Entities
{
    public sealed class HealthStateTests
    {
        [Test]
        public void StatesWithTheSameHealthAndChangeAreEqual()
        {
            Assert.That(new HealthState(40f, -10f).Equals(new HealthState(40f, -10f)), Is.True);
        }

        [TestCase(40f, -5f)]
        [TestCase(41f, -10f)]
        public void StatesDifferingInAnyFieldDiffer(float current, float change)
        {
            Assert.That(new HealthState(40f, -10f).Equals(new HealthState(current, change)), Is.False);
        }

        [Test]
        public void DefaultStateIsEmpty()
        {
            Assert.That(default(HealthState).Equals(new HealthState(0f, 0f)), Is.True);
        }
    }
}
