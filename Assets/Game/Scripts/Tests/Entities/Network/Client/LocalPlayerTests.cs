using System;
using Bw.Entities.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network
{
    public sealed class LocalPlayerTests
    {
        private LifetimeDefinition _lifetime;
        private LocalPlayer _local;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _local = new LocalPlayer();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void LocalPlayerIsUnknownUntilSet()
        {
            Assert.That(_local.Id.Value.IsKnown, Is.False);
            Assert.That(_local.Id.Value.Is(0), Is.False);
            Assert.That(_local.Id.Value.Is(1), Is.False);
        }

        [Test]
        public void SetNamesTheLocalPlayer()
        {
            _local.Set(_lifetime.Lifetime, 5);

            Assert.That(_local.Id.Value.IsKnown, Is.True);
            Assert.That(_local.Id.Value.Is(5), Is.True);
            Assert.That(_local.Id.Value.Is(4), Is.False);
        }

        [Test]
        public void LocalPlayerIsUnknownAgainWhenItsLifetimeEnds()
        {
            var session = new LifetimeDefinition();
            _local.Set(session.Lifetime, 5);

            session.Terminate();

            Assert.That(_local.Id.Value.IsKnown, Is.False);
            Assert.That(_local.Id.Value.Is(5), Is.False);
        }

        [Test]
        public void SetWhileKnownThrows()
        {
            _local.Set(_lifetime.Lifetime, 5);

            Assert.Throws<InvalidOperationException>(() => _local.Set(_lifetime.Lifetime, 6));
            Assert.That(_local.Id.Value.Is(5), Is.True);
        }

        [Test]
        public void SetAgainAfterTheLastLifetimeEnded()
        {
            var first = new LifetimeDefinition();
            _local.Set(first.Lifetime, 5);
            first.Terminate();

            _local.Set(_lifetime.Lifetime, 8);

            Assert.That(_local.Id.Value.Is(8), Is.True);
        }

        [Test]
        public void EveryNamingIsAnnounced()
        {
            var seen = 0;
            _local.Id.Change.Advise(_lifetime.Lifetime, _ => seen++);
            var session = new LifetimeDefinition();

            _local.Set(session.Lifetime, 5);
            session.Terminate();

            Assert.That(seen, Is.EqualTo(2));
        }

        [Test]
        public void KnownIdMatchesOnlyItself()
        {
            var id = LocalPlayerId.Of(-3);

            Assert.That(id.IsKnown, Is.True);
            Assert.That(id.Is(-3), Is.True);
            Assert.That(id.Is(3), Is.False);
        }

        [TestCase(0)]
        [TestCase(int.MinValue)]
        [TestCase(int.MaxValue)]
        public void UnknownIdMatchesNothing(int id)
        {
            Assert.That(LocalPlayerId.Unknown.Is(id), Is.False);
        }
    }
}
