using System;
using Bw.Entities.Network.Prediction.Events;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Events
{
    public sealed class PredictionTargetsTests
    {
        private static readonly ActionId Shot = new(3UL, 40);

        private LifetimeDefinition _lifetime;
        private PredictionTargets<Target, float> _targets;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _targets = new PredictionTargets<Target, float>();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void UnknownTargetAndObjectAreNotFound()
        {
            Assert.That(_targets.TryGet(new Target(), out var byTarget), Is.False);
            Assert.That(byTarget, Is.Null);
            Assert.That(_targets.TryGet(7UL, out var byId), Is.False);
            Assert.That(byId, Is.Null);
        }

        [Test]
        public void RegisteredTargetIsFoundByItselfAndByItsNetworkObject()
        {
            var target = new Target();
            _targets.AddLifetimed(_lifetime.Lifetime, target, 7UL, new RecordingPredictionTarget());

            Assert.That(_targets.TryGet(target, out var byTarget), Is.True);
            Assert.That(_targets.TryGet(7UL, out var byId), Is.True);
            Assert.That(byId, Is.SameAs(byTarget));
        }

        [Test]
        public void TargetPassesShowConfirmAndRejectToEveryPart()
        {
            var target = new Target();
            var vitals = new RecordingPredictionTarget();
            var body = new RecordingPredictionTarget();
            _targets.AddLifetimed(_lifetime.Lifetime, target, 7UL, vitals);
            _targets.AddLifetimed(_lifetime.Lifetime, target, 7UL, body);
            _targets.TryGet(target, out var predictionTarget);

            predictionTarget.Show(new PredictedEffect<float>(Shot, 12f));
            predictionTarget.Confirm(Shot, 10f);
            predictionTarget.Reject(Shot);

            var expected = new[] { "show 3@40 12", "confirm 3@40 10", "reject 3@40" };
            Assert.That(vitals.Calls, Is.EqualTo(expected));
            Assert.That(body.Calls, Is.EqualTo(expected));
        }

        [Test]
        public void SamePartIsRegisteredOnce()
        {
            var target = new Target();
            var part = new RecordingPredictionTarget();
            _targets.AddLifetimed(_lifetime.Lifetime, target, 7UL, part);

            Assert.Throws<InvalidOperationException>(() => _targets.AddLifetimed(_lifetime.Lifetime, target, 7UL, part));
        }

        [Test]
        public void PartOfAnotherNetworkObjectIsRefused()
        {
            var target = new Target();
            _targets.AddLifetimed(_lifetime.Lifetime, target, 7UL, new RecordingPredictionTarget());

            Assert.Throws<InvalidOperationException>(() => _targets.AddLifetimed(_lifetime.Lifetime, target, 8UL, new RecordingPredictionTarget()));
            Assert.That(_targets.TryGet(8UL, out _), Is.False);
        }

        [Test]
        public void NetworkObjectHoldsOneTarget()
        {
            var first = new Target();
            var second = new Target();
            _targets.AddLifetimed(_lifetime.Lifetime, first, 7UL, new RecordingPredictionTarget());

            Assert.Throws<InvalidOperationException>(() => _targets.AddLifetimed(_lifetime.Lifetime, second, 7UL, new RecordingPredictionTarget()));
            Assert.That(_targets.TryGet(second, out _), Is.False);
        }

        [Test]
        public void EndedPartIsNoLongerShown()
        {
            var target = new Target();
            var ended = new LifetimeDefinition();
            var leaving = new RecordingPredictionTarget();
            var staying = new RecordingPredictionTarget();
            _targets.AddLifetimed(ended.Lifetime, target, 7UL, leaving);
            _targets.AddLifetimed(_lifetime.Lifetime, target, 7UL, staying);

            ended.Terminate();
            _targets.TryGet(7UL, out var predictionTarget);
            predictionTarget.Show(new PredictedEffect<float>(Shot, 1f));

            Assert.That(leaving.Calls, Is.Empty);
            Assert.That(staying.Calls, Has.Count.EqualTo(1));
        }

        [Test]
        public void LastEndedPartForgetsBothKeys()
        {
            var target = new Target();
            var ended = new LifetimeDefinition();
            _targets.AddLifetimed(ended.Lifetime, target, 7UL, new RecordingPredictionTarget());

            ended.Terminate();

            Assert.That(_targets.TryGet(target, out _), Is.False);
            Assert.That(_targets.TryGet(7UL, out _), Is.False);
        }

        [Test]
        public void FreedNetworkObjectTakesAnotherTarget()
        {
            var ended = new LifetimeDefinition();
            _targets.AddLifetimed(ended.Lifetime, new Target(), 7UL, new RecordingPredictionTarget());
            ended.Terminate();
            var next = new Target();

            _targets.AddLifetimed(_lifetime.Lifetime, next, 7UL, new RecordingPredictionTarget());

            Assert.That(_targets.TryGet(7UL, out var byId), Is.True);
            Assert.That(_targets.TryGet(next, out var byTarget), Is.True);
            Assert.That(byId, Is.SameAs(byTarget));
        }

        private sealed class Target
        {
        }
    }
}
