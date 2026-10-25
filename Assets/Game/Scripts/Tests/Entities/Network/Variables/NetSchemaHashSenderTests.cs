using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Variables
{
    public sealed class NetSchemaHashSenderTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private NetworkObject _networkObject;
        private RecordingRouter _router;
        private SpawnedObject _spawned;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _networkObject = _objects.Create("Net object").AddComponent<NetworkObject>();
            _router = new RecordingRouter();
            _spawned = new SpawnedObject();
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void NothingIsSentBeforeTheObjectSpawns()
        {
            new NetSchemaHashSender(_lifetime.Lifetime, Schema(_ => { }), _networkObject, _spawned, _router);

            Assert.That(_router.ToServer, Is.Empty);
        }

        [Test]
        public void SpawnSendsTheHashReliablyAsEntryZero()
        {
            new NetSchemaHashSender(_lifetime.Lifetime, Schema(_ => { }), _networkObject, _spawned, _router);

            _spawned.Spawn();

            Assert.That(_router.ToServer, Has.Count.EqualTo(1));
            Assert.That(_router.ToServer[0].Delivery, Is.EqualTo(NetworkDelivery.Reliable));
            var header = Wire.Read<NetworkMessageHeader>(_router.ToServer[0].Bytes);
            Assert.That(header.VarId, Is.EqualTo(0));
            Assert.That(header.NetworkObjectId, Is.EqualTo(_networkObject.NetworkObjectId));
            Assert.That(_router.ToServer[0].Bytes, Has.Length.EqualTo(6));
        }

        [Test]
        public void EverySpawnSendsTheHashAgain()
        {
            new NetSchemaHashSender(_lifetime.Lifetime, Schema(_ => { }), _networkObject, _spawned, _router);

            _spawned.Spawn();
            _spawned.Despawn();
            _spawned.Spawn();

            Assert.That(_router.ToServer, Has.Count.EqualTo(2));
        }

        [Test]
        public void EndedSenderSendsNothing()
        {
            var sender = new LifetimeDefinition();
            new NetSchemaHashSender(sender.Lifetime, Schema(_ => { }), _networkObject, _spawned, _router);
            sender.Terminate();

            _spawned.Spawn();

            Assert.That(_router.ToServer, Is.Empty);
        }

        [Test]
        public void SameDeclarationsGiveTheSameHash()
        {
            static void Declare(INetEntriesSchemaBuilder schema)
            {
                schema.DeclareRequest<int>(NetworkDelivery.Unreliable);
                schema.DeclareResult<bool>(NetworkDelivery.Reliable);
                schema.DeclareProperty(0f, NetworkDelivery.ReliableSequenced, NetworkPermissions.Owner);
            }

            Assert.That(HashOf(Declare), Is.EqualTo(HashOf(Declare)));
        }

        [Test]
        public void InitialValuesDoNotChangeTheHash()
        {
            var zero = HashOf(schema => schema.DeclareProperty(0, NetworkDelivery.Reliable, NetworkPermissions.Server));
            var seven = HashOf(schema => schema.DeclareProperty(7, NetworkDelivery.Reliable, NetworkPermissions.Server));

            Assert.That(seven, Is.EqualTo(zero));
        }

        [Test]
        public void DeclarationOrderChangesTheHash()
        {
            var requestFirst = HashOf(schema =>
            {
                schema.DeclareRequest<int>(NetworkDelivery.Reliable);
                schema.DeclareResult<int>(NetworkDelivery.Reliable);
            });
            var resultFirst = HashOf(schema =>
            {
                schema.DeclareResult<int>(NetworkDelivery.Reliable);
                schema.DeclareRequest<int>(NetworkDelivery.Reliable);
            });

            Assert.That(resultFirst, Is.Not.EqualTo(requestFirst));
        }

        [Test]
        public void DeliveryChangesTheHash()
        {
            var reliable = HashOf(schema => schema.DeclareResult<int>(NetworkDelivery.Reliable));
            var unreliable = HashOf(schema => schema.DeclareResult<int>(NetworkDelivery.Unreliable));

            Assert.That(unreliable, Is.Not.EqualTo(reliable));
        }

        [Test]
        public void PermissionsChangeTheHash()
        {
            var server = HashOf(schema => schema.DeclareProperty(0, NetworkDelivery.Reliable, NetworkPermissions.Server));
            var owner = HashOf(schema => schema.DeclareProperty(0, NetworkDelivery.Reliable, NetworkPermissions.Owner));
            var everyone = HashOf(schema => schema.DeclareProperty(0, NetworkDelivery.Reliable, NetworkPermissions.Everyone));

            Assert.That(owner, Is.Not.EqualTo(server));
            Assert.That(everyone, Is.Not.EqualTo(server));
            Assert.That(everyone, Is.Not.EqualTo(owner));
        }

        [Test]
        public void ValueTypeChangesTheHash()
        {
            var ints = HashOf(schema => schema.DeclareResult<int>(NetworkDelivery.Reliable));
            var floats = HashOf(schema => schema.DeclareResult<float>(NetworkDelivery.Reliable));

            Assert.That(floats, Is.Not.EqualTo(ints));
        }

        [Test]
        public void ExtraEntryChangesTheHash()
        {
            var one = HashOf(schema => schema.DeclareRequest<int>(NetworkDelivery.Reliable));
            var two = HashOf(schema =>
            {
                schema.DeclareRequest<int>(NetworkDelivery.Reliable);
                schema.DeclareRequest<int>(NetworkDelivery.Reliable);
            });

            Assert.That(two, Is.Not.EqualTo(one));
        }

        private static NetEntriesSchema Schema(Action<INetEntriesSchemaBuilder> declare)
        {
            var builder = new NetEntriesSchemaBuilder();
            declare(builder);
            return builder.Build();
        }

        private uint HashOf(Action<INetEntriesSchemaBuilder> declare)
        {
            var router = new RecordingRouter();
            var spawned = new SpawnedObject();
            new NetSchemaHashSender(_lifetime.Lifetime, Schema(declare), _networkObject, spawned, router);
            spawned.Spawn();

            using var reader = new FastBufferReader(router.ToServer[0].Bytes, Allocator.Temp);
            reader.ReadNetworkSerializable(out NetworkMessageHeader _);
            reader.ReadValueSafe(out uint hash);
            return hash;
        }
    }
}
