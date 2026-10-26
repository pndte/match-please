using System;
using System.Collections.Generic;
using System.Linq;
using Bw.Entities.Network;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using Bw.Entities.Players;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using Bw.UseCases;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;
using SpawnedObject = Bw.Tests.Entities.Network.Doubles.SpawnedObject;

namespace Bw.Tests.Entities.Network.Variables
{
    public sealed class NetInboxServerTests
    {
        private const ulong ObjectId = 42;

        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private ClientsById _clients;
        private OwnerRegistry _owners;
        private PlayerDrivers _drivers;
        private RecordingSenders _senders;
        private NetRequestDeclaration<int> _request;
        private NetResultDeclaration<int> _result;
        private NetPropertyDeclaration<int> _everyoneProperty;
        private NetEntries _entries;
        private INetInbox _inbox;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _clients = new ClientsById(_lifetime.Lifetime);
            _owners = new OwnerRegistry();
            _drivers = new PlayerDrivers();
            _senders = new RecordingSenders();
            var schema = new NetEntriesSchemaBuilder();
            _request = schema.DeclareRequest<int>(NetworkDelivery.Reliable);
            _result = schema.DeclareResult<int>(NetworkDelivery.Reliable);
            _everyoneProperty = schema.DeclareProperty(0, NetworkDelivery.Reliable, NetworkPermissions.Everyone);
            var networkObject = _objects.Create("Net object").AddComponent<NetworkObject>();
            _entries = new NetEntries(schema.Build(), new NetSendGuardFactoryServer(new Ownership(), networkObject));
            var guards = new NetReceiveGuardFactoryServer(_lifetime.Lifetime, _owners, _clients, _drivers, new NetReceiveConfig(), networkObject);
            _inbox = new NetInboxServer(_entries, guards, _clients);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void RequestFromItsOwnerIsHeard()
        {
            var client = _clients.Connect(7);
            var player = new Player("Саня");
            _drivers.AddClientDriven(_lifetime.Lifetime, player, client);
            _owners.AddOwner(_lifetime.Lifetime, player);
            var heard = new List<int>();
            _entries.RequestReceiver(_request).Received.Advise(_lifetime.Lifetime, heard.Add);

            Receive(7, 1, 5);

            Assert.That(heard, Is.EqualTo(new[] { 5 }));
        }

        [Test]
        public void RequestFromAClientOwningNothingThrows()
        {
            var client = _clients.Connect(7);
            _drivers.AddClientDriven(_lifetime.Lifetime, new Player("Саня"), client);

            Assert.Throws<InvalidOperationException>(() => Receive(7, 1, 5));
        }

        [Test]
        public void ResultFromAClientThrows()
        {
            _clients.Connect(7);

            Assert.Throws<InvalidOperationException>(() => Receive(7, 2, 5));
        }

        [Test]
        public void UnknownEntryThrows()
        {
            _clients.Connect(7);

            Assert.Throws<InvalidOperationException>(() => Receive(7, 9, 5));
        }

        [Test]
        public void EveryonePropertyFromAClientIsAppliedAndRelayedToTheOtherClients()
        {
            var writer = _clients.Connect(7);
            var other = _clients.Connect(8);
            var spawned = new SpawnedObject(ObjectId);
            var worldStep = new ScriptedWorldStep();
            new NetReplicationServer(_lifetime.Lifetime, _entries, spawned, worldStep, _senders, _clients);
            spawned.Spawn();
            worldStep.Fire(TickPhase.Publish, 10);
            var announced = _senders.All.Count;

            Receive(7, 3, 9);

            var relayed = _senders.All.Skip(announced).ToList();
            Assert.That(_entries.Property(_everyoneProperty).Value, Is.EqualTo(9));
            Assert.That(relayed.Select(sent => sent.Recipient), Is.EqualTo(new[] { other }));
            Assert.That(relayed.Select(sent => sent.Payload), Is.EqualTo(new object[] { 9 }));
            Assert.That(relayed.Select(sent => sent.Recipient), Has.No.Member(writer));
        }

        [Test]
        public void EveryonePropertyFromAClientWhoLeftThrows()
        {
            Assert.Throws<InvalidOperationException>(() => Receive(7, 3, 9));
        }

        private void Receive(ulong senderClientId, ushort varId, int value) =>
            _inbox.Receive(senderClientId, new NetworkMessageHeader { NetworkObjectId = ObjectId, VarId = varId }, new ValueReader<int>(value, senderClientId));
    }
}
