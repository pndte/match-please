using Bw.Entities.Extensions;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Extensions
{
    public sealed class LifetimeTimerExtensionsTests
    {
        private LifetimeDefinition _lifetime;

        [SetUp]
        public void SetUp() =>
            _lifetime = new LifetimeDefinition();

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [TestCase(0f)]
        [TestCase(-1f)]
        public void NoTimeLeftRunsTheHandlerAtOnce(float seconds)
        {
            var calls = 0;

            _lifetime.Lifetime.WhenElapsed(seconds, () => calls++);

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void NoTimeLeftOnAnEndedLifetimeRunsNothing()
        {
            var calls = 0;

            Lifetime.Terminated.WhenElapsed(0f, () => calls++);

            Assert.That(calls, Is.EqualTo(0));
        }

        [Test]
        public void TimeLeftDoesNotRunTheHandlerAtOnce()
        {
            var calls = 0;

            _lifetime.Lifetime.WhenElapsed(1f, () => calls++);

            Assert.That(calls, Is.EqualTo(0));
        }

        [Test]
        public void TimedLifetimeIsAliveBeforeItsTime()
        {
            var timed = _lifetime.Lifetime.CreateNestedFor(1f);

            Assert.That(timed.IsAlive, Is.True);
        }

        [Test]
        public void TimedLifetimeEndsWithItsParent()
        {
            var parent = _lifetime.Lifetime.CreateNested();
            var timed = parent.Lifetime.CreateNestedFor(1f);

            parent.Terminate();

            Assert.That(timed.IsAlive, Is.False);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void TimedLifetimeWithNoTimeIsAlreadyEnded(float seconds)
        {
            var timed = _lifetime.Lifetime.CreateNestedFor(seconds);

            Assert.That(timed.IsAlive, Is.False);
            Assert.That(_lifetime.Lifetime.IsAlive, Is.True);
        }
    }
}
