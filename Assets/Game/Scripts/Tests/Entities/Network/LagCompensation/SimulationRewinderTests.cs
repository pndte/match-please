using System;
using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.LagCompensation.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.LagCompensation
{
    public sealed class SimulationRewinderTests
    {
        private static readonly Client Shooter = new(new ConnectionRequest("Саня", 0, System.Guid.NewGuid(), System.Guid.Empty));

        private LifetimeDefinition _lifetime;
        private SteppedSimulation<float> _simulation;
        private ScriptedWorldStep _worldStep;
        private SimulationHistory<float> _history;
        private RecordingView<float> _view;
        private ScriptedBubble _bubble;
        private SimulationRewinder<float> _rewinder;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _simulation = new SteppedSimulation<float>();
            _worldStep = new ScriptedWorldStep();
            _history = new SimulationHistory<float>(_lifetime.Lifetime, _simulation, new ManualClock(1f / 30f), _worldStep, new LagCompensationConfig { MaxRewindSeconds = 0.4f });
            _view = new RecordingView<float>();
            _bubble = new ScriptedBubble();
            _rewinder = new SimulationRewinder<float>(_history, _simulation, _view, _bubble);
            for (var tick = 10; tick <= 15; tick++)
                Publish(tick, tick * 10f);

            _simulation.Step(999f);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void RewindShowsTheTwoRecordedStatesAroundTheTick()
        {
            _rewinder.Rewind(_lifetime.Lifetime, Seen(12.25, 14.0));

            Assert.That(_view.Shown, Has.Count.EqualTo(1));
            var (from, to, progress) = _view.Shown[0];
            Assert.That((from, to), Is.EqualTo((120f, 130f)));
            Assert.That(progress, Is.EqualTo(0.25f).Within(1e-6f));
        }

        [Test]
        public void ObjectTheShooterPredictedIsRewoundToThePredictedTick()
        {
            _bubble.Holding = true;

            _rewinder.Rewind(_lifetime.Lifetime, Seen(12.25, 14.0));

            Assert.That(_view.Shown[0], Is.EqualTo((140f, 150f, 0f)));
        }

        [Test]
        public void ObjectIsJudgedByItsOwnBubbleHistory()
        {
            _rewinder.Rewind(_lifetime.Lifetime, Seen(12.25, 14.0));

            Assert.That(_bubble.Asked, Is.EqualTo(new[] { ((IClient)Shooter, 14.0) }));
            Assert.That(_view.Shown[0].From, Is.EqualTo(120f));
        }

        [Test]
        public void TickAfterTheNewestRecordUsesTheCurrentState()
        {
            _rewinder.Rewind(_lifetime.Lifetime, Seen(15.5, 15.5));

            Assert.That(_view.Shown[0], Is.EqualTo((150f, 999f, 0.5f)));
        }

        [Test]
        public void TickBeyondTheNewestShowsTheCurrentStateAlone()
        {
            _rewinder.Rewind(_lifetime.Lifetime, Seen(16.0, 16.0));

            Assert.That(_view.Shown[0], Is.EqualTo((999f, 999f, 0f)));
        }

        [Test]
        public void TickWithoutARecordTakesTheNextRecordedOne()
        {
            Publish(17, 170f);

            _rewinder.Rewind(_lifetime.Lifetime, Seen(16.0, 16.0));

            Assert.That(_view.Shown[0], Is.EqualTo((170f, 170f, 0f)));
        }

        [Test]
        public void EndOfTheRewindShowsThePresentAgain()
        {
            var shot = new LifetimeDefinition();
            _rewinder.Rewind(shot.Lifetime, Seen(12.0, 12.0));

            shot.Terminate();

            Assert.That(_view.Shown, Has.Count.EqualTo(2));
            Assert.That(_view.Shown[1], Is.EqualTo((999f, 999f, 0f)));
        }

        [Test]
        public void RewindNeverTouchesTheSimulation()
        {
            var shot = new LifetimeDefinition();

            _rewinder.Rewind(shot.Lifetime, Seen(12.0, 12.0));
            shot.Terminate();

            Assert.That(_simulation.State.Value, Is.EqualTo(999f));
            Assert.That(_simulation.Applied, Is.Empty);
        }

        [Test]
        public void RewindDeeperThanTheHistoryThrows()
        {
            for (var tick = 16; tick <= 30; tick++)
                Publish(tick, tick * 10f);

            Assert.Throws<InvalidOperationException>(() => _rewinder.Rewind(_lifetime.Lifetime, Seen(16.5, 16.5)));
            Assert.That(_view.Shown, Is.Empty);
        }

        [Test]
        public void OldestKeptTickCanStillBeRewound()
        {
            for (var tick = 16; tick <= 30; tick++)
                Publish(tick, tick * 10f);

            _rewinder.Rewind(_lifetime.Lifetime, Seen(17.0, 17.0));

            Assert.That(_view.Shown[0], Is.EqualTo((170f, 180f, 0f)));
        }

        private void Publish(int tick, float state)
        {
            _simulation.Step(state);
            _worldStep.Fire(TickPhase.Publish, tick);
        }

        private static ShooterView Seen(double interpolated, double predicted) =>
            new(interpolated, predicted, PlayerDriver.Client(Shooter));
    }
}
