using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.LagCompensation.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.Entities.Network.LagCompensation
{
    public sealed class RewindableRegistrationTests
    {
        private LifetimeDefinition _lifetime;
        private SpawnedObject _spawned;
        private RecordingRewindable _rewindable;
        private LagCompensator _compensator;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _spawned = new SpawnedObject();
            _rewindable = new RecordingRewindable();
            var clock = new ManualClock();
            clock.Tick(100);
            _compensator = new LagCompensator(clock, new LagCompensationConfig());
            new RewindableRegistration(_lifetime.Lifetime, _spawned, _rewindable, _compensator);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void UnspawnedObjectIsNotRewound()
        {
            Rewind();

            Assert.That(_rewindable.Rewound, Is.Empty);
        }

        [Test]
        public void SpawnedObjectIsRewound()
        {
            _spawned.Spawn();

            Rewind();

            Assert.That(_rewindable.Rewound, Has.Count.EqualTo(1));
        }

        [Test]
        public void DespawnedObjectIsNoLongerRewound()
        {
            _spawned.Spawn();
            _spawned.Despawn();

            Rewind();

            Assert.That(_rewindable.Rewound, Is.Empty);
        }

        [Test]
        public void RespawnedObjectIsRewoundOncePerShot()
        {
            _spawned.Spawn();
            _spawned.Despawn();
            _spawned.Spawn();

            Rewind();

            Assert.That(_rewindable.Rewound, Has.Count.EqualTo(1));
        }

        private void Rewind() =>
            _compensator.Rewind(_lifetime.Lifetime, new ShooterView(99.0, 100.0, PlayerDriver.Server));
    }
}
