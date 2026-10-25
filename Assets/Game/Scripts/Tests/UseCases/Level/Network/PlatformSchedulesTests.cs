using Bw.Entities.Network.Rollback;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.Tests.UseCases.Level.Doubles;
using Bw.UseCases.Level;
using Bw.UseCases.Level.Extensions;
using Bw.UseCases.Level.Network;
using JetBrains.Collections.Viewable;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Level.Network
{
    public sealed class PlatformSchedulesTests
    {
        private static readonly MovingPlatformPath Path = new() { Travel = new Vector2(0f, 10f), TravelSeconds = 2f, PauseSeconds = 0.5f };

        private PhysicsTestWorld _world;
        private Lift _lift;
        private WorldStep _step;

        [SetUp]
        public void SetUp()
        {
            _world = new PhysicsTestWorld();
            _lift = new Lift(_world, Path.Travel);
            _step = new WorldStep(_world.Lifetime, _world.Clock);
            new PhysicsSchedule(_world.Lifetime, _world.Space, _step);
        }

        [TearDown]
        public void TearDown() =>
            _world.Dispose();

        [Test]
        public void ServerScheduleKeepsThePlatformOnItsPathByTheTick()
        {
            new MovingPlatformSchedule(_world.Lifetime, _lift.Platform, Path, _world.Clock, _step);

            for (var tick = 1; tick <= 200; tick++)
            {
                _world.Clock.Tick(tick);
                Assert.That(_lift.Platform.State.Value.Along, Is.EqualTo(Path.AlongAt(tick * (double)_world.Clock.Duration)).Within(1e-4f), $"tick {tick}");
            }
        }

        [Test]
        public void ScheduleFollowsTheTickNumberNotTheCountOfSteps()
        {
            new MovingPlatformSchedule(_world.Lifetime, _lift.Platform, Path, _world.Clock, _step);
            _lift.Platform.Apply(new PlatformState(Path.AlongAt(89 * (double)_world.Clock.Duration)));

            _world.Clock.Tick(90);

            Assert.That(_lift.Platform.State.Value.Along, Is.EqualTo(Path.AlongAt(90 * (double)_world.Clock.Duration)).Within(1e-4f));
        }

        [Test]
        public void PredictedScheduleDrivesOnlyWhilePredicted()
        {
            var predicted = new Predicted();
            new PredictedPlatformSchedule(_world.Lifetime, _lift.Platform, Path, _world.Clock, predicted, _step);

            _world.Clock.Advance(30);

            Assert.That(_lift.Platform.State.Value.Along, Is.Zero.Within(1e-6f));
        }

        [Test]
        public void PredictedScheduleDrivesLikeTheServerOnceInPrediction()
        {
            var predicted = new Predicted();
            new PredictedPlatformSchedule(_world.Lifetime, _lift.Platform, Path, _world.Clock, predicted, _step);
            _world.Clock.Advance(10);

            predicted.Me.Value = true;
            _world.Clock.Advance(30);

            Assert.That(_lift.Platform.State.Value.Along, Is.EqualTo(Path.AlongAt(40 * (double)_world.Clock.Duration)).Within(1e-4f));
        }

        [Test]
        public void PredictedScheduleStopsDrivingWhenPredictionEnds()
        {
            var predicted = new Predicted();
            new PredictedPlatformSchedule(_world.Lifetime, _lift.Platform, Path, _world.Clock, predicted, _step);
            predicted.Me.Value = true;
            _world.Clock.Advance(30);
            var left = _lift.Platform.State.Value.Along;

            var body = _lift.Body;

            predicted.Me.Value = false;
            body.linearVelocity = Vector2.zero;
            _world.Clock.Advance(30);

            Assert.That(_lift.Platform.State.Value.Along, Is.EqualTo(left).Within(1e-6f));
        }

        private sealed class Predicted : IPredictedBy
        {
            public ViewableProperty<bool> Me { get; } = new(false);

            IReadonlyProperty<bool> IPredictedBy.Me => Me;
        }
    }
}
