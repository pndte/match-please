using Bw.Entities.Network.Prediction.Events;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Prediction.Events
{
    public sealed class PredictionTargetRegistrarTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private SpawnedObject _spawned;
        private Target _target;
        private RecordingPredictionTarget _part;
        private PredictionTargets<Target, float> _targets;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _spawned = new SpawnedObject();
            _target = new Target();
            _part = new RecordingPredictionTarget();
            _targets = new PredictionTargets<Target, float>();
            new PredictionTargetRegistrar<Target, float>(
                _lifetime.Lifetime,
                _spawned,
                _objects.Create("character").AddComponent<NetworkObject>(),
                _target,
                _part,
                _targets);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void UnspawnedTargetIsNotAPredictionTarget()
        {
            Assert.That(_targets.TryGet(_target, out _), Is.False);
            Assert.That(_targets.TryGet(0UL, out _), Is.False);
        }

        [Test]
        public void SpawnedTargetIsFoundByItselfAndByItsNetworkObject()
        {
            _spawned.Spawn();

            Assert.That(_targets.TryGet(_target, out var byTarget), Is.True);
            Assert.That(_targets.TryGet(0UL, out var byId), Is.True);
            byId.Reject(new ActionId(1UL, 2));
            Assert.That(byTarget, Is.SameAs(byId));
            Assert.That(_part.Calls, Is.EqualTo(new[] { "reject 1@2" }));
        }

        [Test]
        public void DespawnedTargetIsForgotten()
        {
            _spawned.Spawn();

            _spawned.Despawn();

            Assert.That(_targets.TryGet(_target, out _), Is.False);
            Assert.That(_targets.TryGet(0UL, out _), Is.False);
        }

        [Test]
        public void RespawnedTargetIsFoundAgain()
        {
            _spawned.Spawn();
            _spawned.Despawn();

            _spawned.Spawn();

            Assert.That(_targets.TryGet(_target, out _), Is.True);
        }

        private sealed class Target
        {
        }
    }
}
