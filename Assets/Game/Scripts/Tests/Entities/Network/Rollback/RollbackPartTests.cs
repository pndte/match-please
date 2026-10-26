using Bw.Entities.Network.Rollback;
using Bw.Tests.Doubles;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Rollback
{
    public sealed class RollbackPartTests
    {
        private RestorableValue<int> _state;
        private ServerStates<int> _server;
        private RollbackPart<int> _part;

        [SetUp]
        public void SetUp()
        {
            var config = new WorldRollbackConfig { MaxReplayTicks = 8 };
            _state = new RestorableValue<int>();
            _server = new ServerStates<int>(config.HistoryTicks);
            _part = new RollbackPart<int>(_state, _server, new ExactDivergence<int>(), config);
        }

        [Test]
        public void NoServerStateMeansNoDivergence()
        {
            _state.Simulate(5);
            _part.Save(1);

            Assert.That(_part.TryGetDivergence(out _), Is.False);
        }

        [Test]
        public void ServerStateEqualToThePredictionIsNoDivergence()
        {
            _state.Simulate(5);
            _part.Save(1);

            _server.Record(1, 5);

            Assert.That(_part.TryGetDivergence(out _), Is.False);
        }

        [Test]
        public void ServerStateUnlikeThePredictionDivergesAtItsTick()
        {
            _state.Simulate(5);
            _part.Save(1);
            _state.Simulate(6);
            _part.Save(2);

            _server.Record(2, 60);

            Assert.That(_part.TryGetDivergence(out var tick), Is.True);
            Assert.That(tick, Is.EqualTo(2));
        }

        [Test]
        public void OnlyTheNewestServerStateIsCompared()
        {
            _state.Simulate(5);
            _part.Save(1);
            _state.Simulate(6);
            _part.Save(2);

            _server.Record(1, 50);
            _server.Record(2, 6);

            Assert.That(_part.TryGetDivergence(out _), Is.False);
        }

        [Test]
        public void ServerStateAheadOfEverySavedTickIsNoDivergence()
        {
            _state.Simulate(5);
            _part.Save(1);

            _server.Record(3, 99);

            Assert.That(_part.TryGetDivergence(out _), Is.False);
        }

        [Test]
        public void ServerStateAtAGapBehindSavedTicksDiverges()
        {
            _state.Simulate(5);
            _part.Save(4);

            _server.Record(2, 5);

            Assert.That(_part.TryGetDivergence(out var tick), Is.True);
            Assert.That(tick, Is.EqualTo(2));
        }

        [Test]
        public void RestoreAppliesTheServerStateWhenItHasOne()
        {
            _state.Simulate(5);
            _part.Save(1);
            _server.Record(1, 50);

            _part.Restore(1);

            Assert.That(_state.Applied, Is.EqualTo(new[] { 50 }));
        }

        [Test]
        public void RestoreFallsBackToTheSavedPrediction()
        {
            _state.Simulate(5);
            _part.Save(1);
            _state.Simulate(6);

            _part.Restore(1);

            Assert.That(_state.Applied, Is.EqualTo(new[] { 5 }));
        }

        [Test]
        public void RestoreWithNothingKnownKeepsTheCurrentState()
        {
            _state.Simulate(7);

            _part.Restore(1);

            Assert.That(_state.Applied, Is.EqualTo(new[] { 7 }));
        }

        [Test]
        public void RestoreLaterThanAStaleDivergenceAppliesTheStaleServerState()
        {
            _state.Simulate(5);
            _part.Save(4);
            _server.Record(2, 50);

            _part.Restore(12);

            Assert.That(_state.Applied, Is.EqualTo(new[] { 50 }));
        }

        [Test]
        public void StaleDivergenceIsCorrectedOnce()
        {
            _state.Simulate(5);
            _part.Save(4);
            _server.Record(2, 50);

            _part.Restore(12);

            Assert.That(_part.TryGetDivergence(out _), Is.False);
        }

        [Test]
        public void NewerServerStateAfterAStaleCorrectionIsComparedAgain()
        {
            _state.Simulate(5);
            _part.Save(4);
            _server.Record(2, 50);
            _part.Restore(12);
            _part.Save(13);

            _server.Record(13, 60);

            Assert.That(_part.TryGetDivergence(out var tick), Is.True);
            Assert.That(tick, Is.EqualTo(13));
        }

        [Test]
        public void RestoreLaterThanAnAgreeingServerStateKeepsThePrediction()
        {
            _state.Simulate(5);
            _part.Save(2);
            _server.Record(2, 5);
            _state.Simulate(6);
            _part.Save(3);

            _part.Restore(3);

            Assert.That(_state.Applied, Is.EqualTo(new[] { 6 }));
        }

        [Test]
        public void RestoredServerStateBecomesThePredictionAtItsTick()
        {
            _state.Simulate(5);
            _part.Save(1);
            _server.Record(1, 50);

            _part.Restore(1);

            Assert.That(_part.TryGetDivergence(out _), Is.False);
        }
    }
}
