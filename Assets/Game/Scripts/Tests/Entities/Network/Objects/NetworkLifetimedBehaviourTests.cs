using Bw.Entities.Network.Objects;
using Bw.Tests.Doubles;
using NUnit.Framework;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Objects
{
    public sealed class NetworkLifetimedBehaviourTests
    {
        private HiddenObjects _objects;
        private NetworkLifetimedBehaviour _behaviour;

        [SetUp]
        public void SetUp()
        {
            _objects = new HiddenObjects();
            var root = _objects.Create("Net object");
            root.AddComponent<NetworkObject>();
            _behaviour = root.AddComponent<NetworkLifetimedBehaviour>();
        }

        [TearDown]
        public void TearDown() =>
            _objects.Dispose();

        [Test]
        public void ObjectIsNotSpawnedAtFirst()
        {
            Assert.That(_behaviour.SpawnedLifetime.Value.IsAlive, Is.False);
        }

        [Test]
        public void SpawnOpensTheSpawnedLifetime()
        {
            _behaviour.OnNetworkSpawn();

            Assert.That(_behaviour.SpawnedLifetime.Value.IsAlive, Is.True);
        }

        [Test]
        public void DespawnEndsTheSpawnedLifetime()
        {
            _behaviour.OnNetworkSpawn();
            var spawned = _behaviour.SpawnedLifetime.Value;

            _behaviour.OnNetworkDespawn();

            Assert.That(spawned.IsAlive, Is.False);
            Assert.That(_behaviour.SpawnedLifetime.Value.IsAlive, Is.False);
        }

        [Test]
        public void EverySpawnGetsAFreshLifetime()
        {
            _behaviour.OnNetworkSpawn();
            var first = _behaviour.SpawnedLifetime.Value;
            _behaviour.OnNetworkDespawn();

            _behaviour.OnNetworkSpawn();

            Assert.That(first.IsAlive, Is.False);
            Assert.That(_behaviour.SpawnedLifetime.Value.IsAlive, Is.True);
        }
    }
}
