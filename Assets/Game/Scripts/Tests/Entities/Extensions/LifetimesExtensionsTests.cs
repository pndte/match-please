using System.Collections.Generic;
using Bw.Entities.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Extensions
{
    public sealed class LifetimesExtensionsTests
    {
        private LifetimeDefinition _lifetime;
        private ViewableProperty<Lifetime> _property;
        private List<Lifetime> _seen;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _property = new ViewableProperty<Lifetime>(Lifetime.Terminated);
            _seen = new List<Lifetime>();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void WhenAliveSeesEveryAliveValue()
        {
            _property.WhenAlive(_lifetime.Lifetime, _seen.Add);
            var first = Alive();
            var second = Alive();

            _property.Value = first;
            _property.Value = Lifetime.Terminated;
            _property.Value = second;

            Assert.That(_seen, Is.EqualTo(new[] { first, second }));
        }

        [Test]
        public void WhenAliveSeesTheCurrentAliveValueAtOnce()
        {
            var current = Alive();
            _property.Value = current;

            _property.WhenAlive(_lifetime.Lifetime, _seen.Add);

            Assert.That(_seen, Is.EqualTo(new[] { current }));
        }

        [Test]
        public void WhenAliveSkipsAnEndedValue()
        {
            var ended = _lifetime.Lifetime.CreateNested();
            ended.Terminate();
            _property.WhenAlive(_lifetime.Lifetime, _seen.Add);

            _property.Value = ended.Lifetime;

            Assert.That(_seen, Is.Empty);
        }

        [Test]
        public void WhenAliveStopsWithItsLifetime()
        {
            var lifetime = _lifetime.Lifetime.CreateNested();
            _property.WhenAlive(lifetime.Lifetime, _seen.Add);
            lifetime.Terminate();

            _property.Value = Alive();

            Assert.That(_seen, Is.Empty);
        }

        [Test]
        public void WhenAliveOnceSeesOnlyTheFirstAliveValue()
        {
            _property.WhenAliveOnce(_lifetime.Lifetime, _seen.Add);
            var first = Alive();

            _property.Value = first;
            _property.Value = Alive();

            Assert.That(_seen, Is.EqualTo(new[] { first }));
        }

        [Test]
        public void WhenAliveOnceWaitsPastEndedValues()
        {
            _property.WhenAliveOnce(_lifetime.Lifetime, _seen.Add);
            var ended = _lifetime.Lifetime.CreateNested();
            ended.Terminate();
            var alive = Alive();

            _property.Value = ended.Lifetime;
            _property.Value = alive;

            Assert.That(_seen, Is.EqualTo(new[] { alive }));
        }

        [Test]
        public void WhenAliveOnceTakesTheCurrentAliveValueAndNothingMore()
        {
            var current = Alive();
            _property.Value = current;

            _property.WhenAliveOnce(_lifetime.Lifetime, _seen.Add);
            _property.Value = Alive();

            Assert.That(_seen, Is.EqualTo(new[] { current }));
        }

        [Test]
        public void WhenAliveOnceStopsWithItsLifetime()
        {
            var lifetime = _lifetime.Lifetime.CreateNested();
            _property.WhenAliveOnce(lifetime.Lifetime, _seen.Add);
            lifetime.Terminate();

            _property.Value = Alive();

            Assert.That(_seen, Is.Empty);
        }

        private Lifetime Alive() =>
            _lifetime.Lifetime.CreateNested().Lifetime;
    }
}
