using System;
using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.LagCompensation.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.Entities.Network.LagCompensation
{
    public sealed class LagCompensatorTests
    {
        private LifetimeDefinition _lifetime;
        private ManualClock _clock;
        private LagCompensationConfig _config;
        private LagCompensator _compensator;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _clock = new ManualClock(1f / 30f);
            _config = new LagCompensationConfig { MaxRewindSeconds = 0.4f };
            _compensator = new LagCompensator(_clock, _config);
            _clock.Tick(100);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void RewindReachesEveryTrackedObject()
        {
            var first = new RecordingRewindable();
            var second = new RecordingRewindable();
            _compensator.Track(_lifetime.Lifetime, first);
            _compensator.Track(_lifetime.Lifetime, second);
            var shot = new LifetimeDefinition();

            _compensator.Rewind(shot.Lifetime, View(95.5, 99.0));

            Assert.That(first.Rewound, Has.Count.EqualTo(1));
            Assert.That(second.Rewound, Has.Count.EqualTo(1));
            Assert.That(first.Rewound[0].Lifetime, Is.EqualTo(shot.Lifetime));
            Assert.That(first.Rewound[0].View.Interpolated, Is.EqualTo(95.5));
            Assert.That(first.Rewound[0].View.Predicted, Is.EqualTo(99.0));
        }

        [TestCase(80.0, 120.0, 88.0, 100.0)]
        [TestCase(88.0, 100.0, 88.0, 100.0)]
        [TestCase(87.9, 100.1, 88.0, 100.0)]
        [TestCase(-1000.0, -1000.0, 88.0, 88.0)]
        public void RewindNeverGoesDeeperThanTheConfiguredTimeNorPastTheCurrentTick(
            double interpolated,
            double predicted,
            double rewoundInterpolated,
            double rewoundPredicted)
        {
            var rewindable = new RecordingRewindable();
            _compensator.Track(_lifetime.Lifetime, rewindable);

            _compensator.Rewind(_lifetime.Lifetime, View(interpolated, predicted));

            Assert.That(rewindable.Rewound[0].View.Interpolated, Is.EqualTo(rewoundInterpolated));
            Assert.That(rewindable.Rewound[0].View.Predicted, Is.EqualTo(rewoundPredicted));
        }

        [Test]
        public void DepthIsReadOnEveryRewind()
        {
            var rewindable = new RecordingRewindable();
            _compensator.Track(_lifetime.Lifetime, rewindable);
            _config.MaxRewindSeconds = 0.1f;

            _compensator.Rewind(_lifetime.Lifetime, View(80.0, 80.0));

            Assert.That(rewindable.Rewound[0].View.Interpolated, Is.EqualTo(97.0));
        }

        [TestCase(double.NaN, 99.0)]
        [TestCase(99.0, double.NaN)]
        [TestCase(double.PositiveInfinity, 99.0)]
        [TestCase(99.0, double.NegativeInfinity)]
        public void RewindToATickThatIsNotFiniteThrows(double interpolated, double predicted)
        {
            var rewindable = new RecordingRewindable();
            _compensator.Track(_lifetime.Lifetime, rewindable);

            Assert.Throws<ArgumentOutOfRangeException>(() => _compensator.Rewind(_lifetime.Lifetime, View(interpolated, predicted)));
            Assert.That(rewindable.Rewound, Is.Empty);
        }

        [Test]
        public void ObjectIsTrackedOnce()
        {
            var rewindable = new RecordingRewindable();
            _compensator.Track(_lifetime.Lifetime, rewindable);

            Assert.Throws<InvalidOperationException>(() => _compensator.Track(_lifetime.Lifetime, rewindable));
        }

        [Test]
        public void ObjectLeavesWithItsLifetime()
        {
            var tracked = new LifetimeDefinition();
            var rewindable = new RecordingRewindable();
            _compensator.Track(tracked.Lifetime, rewindable);

            tracked.Terminate();
            _compensator.Rewind(_lifetime.Lifetime, View(95.0, 95.0));

            Assert.That(rewindable.Rewound, Is.Empty);
        }

        [Test]
        public void ObjectThatLeftCanBeTrackedAgain()
        {
            var tracked = new LifetimeDefinition();
            var rewindable = new RecordingRewindable();
            _compensator.Track(tracked.Lifetime, rewindable);
            tracked.Terminate();

            _compensator.Track(_lifetime.Lifetime, rewindable);
            _compensator.Rewind(_lifetime.Lifetime, View(95.0, 95.0));

            Assert.That(rewindable.Rewound, Has.Count.EqualTo(1));
        }

        [Test]
        public void RewindWithNothingTrackedDoesNothing()
        {
            Assert.DoesNotThrow(() => _compensator.Rewind(_lifetime.Lifetime, View(95.0, 95.0)));
        }

        private static ShooterView View(double interpolated, double predicted) =>
            new(interpolated, predicted, PlayerDriver.Server);
    }
}
