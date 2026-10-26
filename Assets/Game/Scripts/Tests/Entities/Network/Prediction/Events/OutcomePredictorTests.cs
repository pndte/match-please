using System;
using Bw.Entities.Network.Prediction.Events;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Prediction.Events
{
    public sealed class OutcomePredictorTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private PredictionTargets<Target, float> _targets;
        private OutcomePredictor<Target, float> _predictor;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _targets = new PredictionTargets<Target, float>();
            _predictor = new OutcomePredictor<Target, float>(_objects.Create("weapon").AddComponent<NetworkObject>(), _targets);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void NothingIsPendingAtFirst()
        {
            Assert.That(_predictor.Pending.Value, Is.False);
        }

        [Test]
        public void PredictionOnATargetNobodyRegisteredThrowsAndShowsNothing()
        {
            Assert.Throws<InvalidOperationException>(() => _predictor.Predict(12, new Target(), 30f));
            Assert.That(_predictor.Pending.Value, Is.False);
        }

        [Test]
        public void PredictionShowsTheEffectUnderTheInitiatorsAction()
        {
            var (target, part, _) = Registered(7UL);

            _predictor.Predict(12, target, 30f);

            Assert.That(part.Calls, Is.EqualTo(new[] { "show 0@12 30" }));
            Assert.That(_predictor.Pending.Value, Is.True);
        }

        [Test]
        public void AnswerConfirmsAnAffectedTargetWithTheRealEffect()
        {
            var (target, part, predictionTarget) = Registered(7UL);
            _predictor.Predict(12, target, 30f);

            _predictor.Answer(12, new[] { new AffectedTarget<IPredictionTarget<float>, float>(predictionTarget, 25f) });

            Assert.That(part.Calls, Is.EqualTo(new[] { "show 0@12 30", "confirm 0@12 25" }));
            Assert.That(_predictor.Pending.Value, Is.False);
        }

        [Test]
        public void AnswerRejectsATargetTheServerDidNotAffect()
        {
            var (hit, hitPart, hitTarget) = Registered(7UL);
            var (missed, missedPart, _) = Registered(8UL);
            _predictor.Predict(12, hit, 30f);
            _predictor.Predict(12, missed, 30f);

            _predictor.Answer(12, new[] { new AffectedTarget<IPredictionTarget<float>, float>(hitTarget, 30f) });

            Assert.That(hitPart.Calls[^1], Is.EqualTo("confirm 0@12 30"));
            Assert.That(missedPart.Calls[^1], Is.EqualTo("reject 0@12"));
        }

        [Test]
        public void MissRejectsEveryPredictionOfItsTick()
        {
            var (target, part, _) = Registered(7UL);
            _predictor.Predict(12, target, 30f);

            _predictor.Answer(12, Array.Empty<AffectedTarget<IPredictionTarget<float>, float>>());

            Assert.That(part.Calls[^1], Is.EqualTo("reject 0@12"));
            Assert.That(_predictor.Pending.Value, Is.False);
        }

        [Test]
        public void AnswerLeavesOtherTicksWaiting()
        {
            var (target, part, _) = Registered(7UL);
            _predictor.Predict(12, target, 30f);
            _predictor.Predict(13, target, 20f);

            _predictor.Answer(12, Array.Empty<AffectedTarget<IPredictionTarget<float>, float>>());

            Assert.That(part.Calls, Is.EqualTo(new[] { "show 0@12 30", "show 0@13 20", "reject 0@12" }));
            Assert.That(_predictor.Pending.Value, Is.True);
        }

        [Test]
        public void AnswerForATickNobodyPredictedChangesNothing()
        {
            var (target, part, predictionTarget) = Registered(7UL);
            _predictor.Predict(12, target, 30f);

            _predictor.Answer(20, new[] { new AffectedTarget<IPredictionTarget<float>, float>(predictionTarget, 30f) });

            Assert.That(part.Calls, Is.EqualTo(new[] { "show 0@12 30" }));
            Assert.That(_predictor.Pending.Value, Is.True);
        }

        [Test]
        public void TargetShownTwiceInATickIsAnsweredOnce()
        {
            var (target, part, predictionTarget) = Registered(7UL);
            _predictor.Predict(12, target, 10f);
            _predictor.Predict(12, target, 10f);

            _predictor.Answer(12, new[] { new AffectedTarget<IPredictionTarget<float>, float>(predictionTarget, 20f) });

            Assert.That(part.Calls, Is.EqualTo(new[] { "show 0@12 10", "show 0@12 10", "confirm 0@12 20" }));
        }

        [Test]
        public void SecondAnswerOfATickChangesNothing()
        {
            var (target, part, predictionTarget) = Registered(7UL);
            _predictor.Predict(12, target, 30f);
            _predictor.Answer(12, new[] { new AffectedTarget<IPredictionTarget<float>, float>(predictionTarget, 30f) });

            _predictor.Answer(12, Array.Empty<AffectedTarget<IPredictionTarget<float>, float>>());

            Assert.That(part.Calls, Is.EqualTo(new[] { "show 0@12 30", "confirm 0@12 30" }));
        }

        [Test]
        public void ExpiryRejectsOnlyPredictionsOlderThanItsTick()
        {
            var (target, part, _) = Registered(7UL);
            _predictor.Predict(10, target, 1f);
            _predictor.Predict(11, target, 2f);
            _predictor.Predict(12, target, 3f);

            _predictor.ExpireBefore(12);

            Assert.That(part.Calls, Is.EquivalentTo(new[] { "show 0@10 1", "show 0@11 2", "show 0@12 3", "reject 0@10", "reject 0@11" }));
            Assert.That(_predictor.Pending.Value, Is.True);
        }

        [Test]
        public void ExpiryOfEveryPredictionEndsPending()
        {
            var (target, _, _) = Registered(7UL);
            _predictor.Predict(10, target, 1f);

            _predictor.ExpireBefore(11);

            Assert.That(_predictor.Pending.Value, Is.False);
        }

        [Test]
        public void ExpiredPredictionIsNotAnsweredAgain()
        {
            var (target, part, predictionTarget) = Registered(7UL);
            _predictor.Predict(10, target, 1f);
            _predictor.ExpireBefore(11);

            _predictor.Answer(10, new[] { new AffectedTarget<IPredictionTarget<float>, float>(predictionTarget, 1f) });

            Assert.That(part.Calls, Is.EqualTo(new[] { "show 0@10 1", "reject 0@10" }));
        }

        private (Target Target, RecordingPredictionTarget Part, IPredictionTarget<float> PredictionTarget) Registered(ulong networkObjectId)
        {
            var target = new Target();
            var part = new RecordingPredictionTarget();
            _targets.AddLifetimed(_lifetime.Lifetime, target, networkObjectId, part);
            _targets.TryGet(target, out var predictionTarget);
            return (target, part, predictionTarget);
        }

        private sealed class Target
        {
        }
    }
}
