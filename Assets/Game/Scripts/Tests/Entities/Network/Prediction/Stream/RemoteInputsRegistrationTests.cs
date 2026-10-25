using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class RemoteInputsRegistrationTests
    {
        private LifetimeDefinition _lifetime;
        private Toggle _predicted;
        private RemoteInputs<int> _inputs;
        private RecordingRollbackParts _parts;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _predicted = new Toggle();
            _inputs = new RemoteInputs<int>(_lifetime.Lifetime, new ManualResults<TickedInput<int>>(), new Same(), new Unequal(), new WorldRollbackConfig());
            _parts = new RecordingRollbackParts();
            new RemoteInputsRegistration<int>(_lifetime.Lifetime, _predicted, _inputs, _parts);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void InputsOfAnObjectNotPredictedAreNotWatched()
        {
            Assert.That(_parts.Watched, Is.Empty);
        }

        [Test]
        public void InputsOfAPredictedObjectAreWatchedWhileItIsPredicted()
        {
            _predicted.Set(true);

            Assert.That(_parts.Watched, Has.Count.EqualTo(1));
            Assert.That(_parts.Watched[0].Source, Is.SameAs(_inputs));
            Assert.That(_parts.Watched[0].Lifetime.IsAlive, Is.True);
            Assert.That(_parts.Tracked, Is.Empty);
        }

        [Test]
        public void LeavingThePredictionEndsTheWatch()
        {
            _predicted.Set(true);

            _predicted.Set(false);

            Assert.That(_parts.Watched[0].Lifetime.IsAlive, Is.False);
        }

        [Test]
        public void EveryPredictionGetsItsOwnWatch()
        {
            _predicted.Set(true);
            _predicted.Set(false);

            _predicted.Set(true);

            Assert.That(_parts.Watched, Has.Count.EqualTo(2));
            Assert.That(_parts.Watched[1].Lifetime.IsAlive, Is.True);
        }

        [Test]
        public void EndedRegistrationEndsTheWatch()
        {
            _predicted.Set(true);

            _lifetime.Terminate();

            Assert.That(_parts.Watched[0].Lifetime.IsAlive, Is.False);
        }

        private sealed class Same : IInputContinuation<int>
        {
            public int Continued(int last, int ticks) =>
                last;
        }

        private sealed class Unequal : IInputDivergence<int>
        {
            public bool Diverged(int guessed, int relayed) =>
                guessed != relayed;
        }
    }
}
