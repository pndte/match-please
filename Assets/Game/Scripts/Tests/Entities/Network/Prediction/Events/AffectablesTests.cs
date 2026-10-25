using System;
using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Events
{
    public sealed class AffectablesTests
    {
        private static readonly StateCause Hit = StateCause.Of(new ActionId(3UL, 40));

        private LifetimeDefinition _lifetime;
        private Affectables<Target, float> _affectables;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _affectables = new Affectables<Target, float>();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void UnknownTargetIsNotFound()
        {
            var found = _affectables.TryGet(new Target(), out var id, out var affectable);

            Assert.That(found, Is.False);
            Assert.That(id, Is.EqualTo(0UL));
            Assert.That(affectable, Is.Null);
        }

        [Test]
        public void RegisteredTargetIsFoundUnderItsNetworkObject()
        {
            var target = new Target();
            _affectables.AddLifetimed(_lifetime.Lifetime, target, 7UL, new RecordingAffectable<float>());

            var found = _affectables.TryGet(target, out var id, out var affectable);

            Assert.That(found, Is.True);
            Assert.That(id, Is.EqualTo(7UL));
            Assert.That(affectable, Is.Not.Null);
        }

        [Test]
        public void TargetPassesAnEffectToEveryPartInOrder()
        {
            var order = new List<string>();
            var target = new Target();
            var vitals = new RecordingAffectable<float>(order, "vitals");
            var body = new RecordingAffectable<float>(order, "body");
            _affectables.AddLifetimed(_lifetime.Lifetime, target, 7UL, vitals);
            _affectables.AddLifetimed(_lifetime.Lifetime, target, 7UL, body);
            _affectables.TryGet(target, out _, out var affectable);

            affectable.Affect(Hit, 12f);

            Assert.That(order, Is.EqualTo(new[] { "vitals affected", "body affected" }));
            Assert.That(vitals.Affected, Is.EqualTo(new[] { (Hit, 12f) }));
            Assert.That(body.Affected, Is.EqualTo(new[] { (Hit, 12f) }));
        }

        [Test]
        public void EffectReachesOnlyItsOwnTarget()
        {
            var first = new Target();
            var second = new Target();
            var firstPart = new RecordingAffectable<float>();
            var secondPart = new RecordingAffectable<float>();
            _affectables.AddLifetimed(_lifetime.Lifetime, first, 1UL, firstPart);
            _affectables.AddLifetimed(_lifetime.Lifetime, second, 2UL, secondPart);
            _affectables.TryGet(first, out _, out var affectable);

            affectable.Affect(Hit, 5f);

            Assert.That(firstPart.Affected, Has.Count.EqualTo(1));
            Assert.That(secondPart.Affected, Is.Empty);
        }

        [Test]
        public void SamePartIsRegisteredOnce()
        {
            var target = new Target();
            var part = new RecordingAffectable<float>();
            _affectables.AddLifetimed(_lifetime.Lifetime, target, 7UL, part);

            Assert.Throws<InvalidOperationException>(() => _affectables.AddLifetimed(_lifetime.Lifetime, target, 7UL, part));
        }

        [Test]
        public void PartOfAnotherNetworkObjectIsRefused()
        {
            var target = new Target();
            var part = new RecordingAffectable<float>();
            var stranger = new RecordingAffectable<float>();
            _affectables.AddLifetimed(_lifetime.Lifetime, target, 7UL, part);

            Assert.Throws<InvalidOperationException>(() => _affectables.AddLifetimed(_lifetime.Lifetime, target, 8UL, stranger));
            _affectables.TryGet(target, out var id, out var affectable);
            affectable.Affect(Hit, 1f);
            Assert.That(id, Is.EqualTo(7UL));
            Assert.That(stranger.Affected, Is.Empty);
        }

        [Test]
        public void EndedPartIsNoLongerAffected()
        {
            var target = new Target();
            var ended = new LifetimeDefinition();
            var leaving = new RecordingAffectable<float>();
            var staying = new RecordingAffectable<float>();
            _affectables.AddLifetimed(ended.Lifetime, target, 7UL, leaving);
            _affectables.AddLifetimed(_lifetime.Lifetime, target, 7UL, staying);

            ended.Terminate();
            _affectables.TryGet(target, out _, out var affectable);
            affectable.Affect(Hit, 1f);

            Assert.That(leaving.Affected, Is.Empty);
            Assert.That(staying.Affected, Has.Count.EqualTo(1));
        }

        [Test]
        public void TargetIsForgottenWithItsLastPart()
        {
            var target = new Target();
            var ended = new LifetimeDefinition();
            _affectables.AddLifetimed(ended.Lifetime, target, 7UL, new RecordingAffectable<float>());
            _affectables.AddLifetimed(ended.Lifetime, target, 7UL, new RecordingAffectable<float>());

            ended.Terminate();

            Assert.That(_affectables.TryGet(target, out _, out _), Is.False);
        }

        [Test]
        public void ForgottenTargetComesBackUnderANewNetworkObject()
        {
            var target = new Target();
            var part = new RecordingAffectable<float>();
            var firstSpawn = new LifetimeDefinition();
            _affectables.AddLifetimed(firstSpawn.Lifetime, target, 7UL, part);
            firstSpawn.Terminate();

            _affectables.AddLifetimed(_lifetime.Lifetime, target, 9UL, part);

            Assert.That(_affectables.TryGet(target, out var id, out _), Is.True);
            Assert.That(id, Is.EqualTo(9UL));
        }

        private sealed class Target
        {
        }
    }
}
