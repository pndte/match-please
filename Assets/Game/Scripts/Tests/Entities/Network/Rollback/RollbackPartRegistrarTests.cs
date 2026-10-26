using Bw.Entities.Network.Rollback;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Rollback
{
    public sealed class RollbackPartRegistrarTests
    {
        private LifetimeDefinition _lifetime;
        private Toggle _predicted;
        private RollbackPart<int> _part;
        private RecordingRollbackParts _parts;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _predicted = new Toggle();
            var config = new WorldRollbackConfig();
            _part = new RollbackPart<int>(new RestorableValue<int>(), new ServerStates<int>(config.HistoryTicks), new ExactDivergence<int>(), config);
            _parts = new RecordingRollbackParts();
            new RollbackPartRegistrar<int>(_lifetime.Lifetime, _predicted, _part, _parts);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void PartOfAnObjectNotPredictedIsNotTracked()
        {
            Assert.That(_parts.Tracked, Is.Empty);
        }

        [Test]
        public void PartOfAPredictedObjectIsTrackedWhileItIsPredicted()
        {
            _predicted.Set(true);

            Assert.That(_parts.Tracked, Has.Count.EqualTo(1));
            Assert.That(_parts.Tracked[0].Part, Is.SameAs(_part));
            Assert.That(_parts.Tracked[0].Lifetime.IsAlive, Is.True);
            Assert.That(_parts.Watched, Is.Empty);
        }

        [Test]
        public void LeavingThePredictionEndsTheTracking()
        {
            _predicted.Set(true);

            _predicted.Set(false);

            Assert.That(_parts.Tracked[0].Lifetime.IsAlive, Is.False);
        }

        [Test]
        public void EveryPredictionTracksThePartAgain()
        {
            _predicted.Set(true);
            _predicted.Set(false);

            _predicted.Set(true);

            Assert.That(_parts.Tracked, Has.Count.EqualTo(2));
            Assert.That(_parts.Tracked[1].Lifetime.IsAlive, Is.True);
        }

        [Test]
        public void EndedRegistrationEndsTheTracking()
        {
            _predicted.Set(true);

            _lifetime.Terminate();

            Assert.That(_parts.Tracked[0].Lifetime.IsAlive, Is.False);
        }

        [Test]
        public void ObjectPredictedFromTheStartIsTrackedAtOnce()
        {
            var parts = new RecordingRollbackParts();

            new RollbackPartRegistrar<int>(_lifetime.Lifetime, new Toggle(true), _part, parts);

            Assert.That(parts.Tracked, Has.Count.EqualTo(1));
        }
    }
}
