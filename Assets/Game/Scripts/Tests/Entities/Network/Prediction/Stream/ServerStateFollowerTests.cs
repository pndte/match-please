using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class ServerStateFollowerTests
    {
        private LifetimeDefinition _lifetime;
        private Toggle _predicted;
        private ScriptedWorldStep _worldStep;
        private ServerStates<int> _server;
        private RestorableValue<int> _state;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _predicted = new Toggle();
            _worldStep = new ScriptedWorldStep();
            _server = new ServerStates<int>(8);
            _state = new RestorableValue<int>();
            new ServerStateFollower<int>(_lifetime.Lifetime, _predicted, _worldStep, _server, _state);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void ObjectNotPredictedTakesTheNewestServerStateAtPrepare()
        {
            _server.Record(3, 30);
            _server.Record(5, 50);
            _server.Record(4, 40);

            _worldStep.Fire(TickPhase.Prepare, 10);

            Assert.That(_state.Applied, Is.EqualTo(new[] { 50 }));
        }

        [Test]
        public void NothingFromTheServerChangesNothing()
        {
            _worldStep.Fire(TickPhase.Prepare, 10);

            Assert.That(_state.Applied, Is.Empty);
        }

        [Test]
        public void SameServerStateIsTakenOnce()
        {
            _server.Record(5, 50);
            _worldStep.Fire(TickPhase.Prepare, 10);

            _worldStep.Fire(TickPhase.Prepare, 11);

            Assert.That(_state.Applied, Is.EqualTo(new[] { 50 }));
        }

        [Test]
        public void NewerServerStateIsTakenOnTheNextPrepare()
        {
            _server.Record(5, 50);
            _worldStep.Fire(TickPhase.Prepare, 10);
            _server.Record(6, 60);

            _worldStep.Fire(TickPhase.Prepare, 11);

            Assert.That(_state.Applied, Is.EqualTo(new[] { 50, 60 }));
        }

        [Test]
        public void OlderServerStateNeverReplacesAFollowedOne()
        {
            _server.Record(5, 50);
            _worldStep.Fire(TickPhase.Prepare, 10);
            _server.Record(4, 40);

            _worldStep.Fire(TickPhase.Prepare, 11);

            Assert.That(_state.Applied, Is.EqualTo(new[] { 50 }));
        }

        [Test]
        public void PredictedObjectIgnoresTheServer()
        {
            _predicted.Set(true);
            _server.Record(5, 50);

            _worldStep.Fire(TickPhase.Prepare, 10);

            Assert.That(_state.Applied, Is.Empty);
        }

        [Test]
        public void ObjectLeavingThePredictionFollowsTheServerAgain()
        {
            _predicted.Set(true);
            _server.Record(5, 50);
            _worldStep.Fire(TickPhase.Prepare, 10);

            _predicted.Set(false);
            _worldStep.Fire(TickPhase.Prepare, 11);

            Assert.That(_state.Applied, Is.EqualTo(new[] { 50 }));
        }

        [TestCase(TickPhase.Early)]
        [TestCase(TickPhase.Default)]
        [TestCase(TickPhase.Late)]
        [TestCase(TickPhase.Publish)]
        public void OnlyPrepareFollows(TickPhase phase)
        {
            _server.Record(5, 50);

            _worldStep.Fire(phase, 10);

            Assert.That(_state.Applied, Is.Empty);
        }
    }
}
