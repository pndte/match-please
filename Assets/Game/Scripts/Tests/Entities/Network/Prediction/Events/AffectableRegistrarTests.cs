using Bw.Entities.Network.Prediction.Events;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Prediction.Events
{
    public sealed class AffectableRegistrarTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private SpawnedObject _spawned;
        private Target _target;
        private RecordingAffectable<float> _part;
        private Affectables<Target, float> _affectables;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _spawned = new SpawnedObject();
            _target = new Target();
            _part = new RecordingAffectable<float>();
            _affectables = new Affectables<Target, float>();
            new AffectableRegistrar<Target, float>(
                _lifetime.Lifetime,
                _spawned,
                _objects.Create("crate").AddComponent<NetworkObject>(),
                _target,
                _part,
                _affectables);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void UnspawnedTargetIsNotAffectable()
        {
            Assert.That(_affectables.TryGet(_target, out _, out _), Is.False);
        }

        [Test]
        public void SpawnedTargetIsAffectableUnderItsNetworkObject()
        {
            _spawned.Spawn();

            Assert.That(_affectables.TryGet(_target, out var id, out var affectable), Is.True);
            Assert.That(id, Is.EqualTo(0UL));
            affectable.Affect(StateCause.None, 3f);
            Assert.That(_part.Affected, Has.Count.EqualTo(1));
        }

        [Test]
        public void DespawnedTargetIsNoLongerAffectable()
        {
            _spawned.Spawn();

            _spawned.Despawn();

            Assert.That(_affectables.TryGet(_target, out _, out _), Is.False);
        }

        [Test]
        public void RespawnedTargetIsAffectableAgain()
        {
            _spawned.Spawn();
            _spawned.Despawn();

            _spawned.Spawn();

            Assert.That(_affectables.TryGet(_target, out _, out _), Is.True);
        }

        [Test]
        public void EndedRegistrationNoLongerRegistersSpawns()
        {
            _lifetime.Terminate();

            _spawned.Spawn();

            Assert.That(_affectables.TryGet(_target, out _, out _), Is.False);
        }

        private sealed class Target
        {
        }
    }
}
