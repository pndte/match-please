using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Doubles;
using Bw.UseCases;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;

namespace Bw.Tests.Entities.Network.Variables
{
    public sealed class NetSchemaHashCheckerTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private ClientsById _clients;
        private NetEntriesSchema _schema;
        private Disconnections _disconnections;
        private INetInbox _inbox;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _clients = new ClientsById(_lifetime.Lifetime);
            _disconnections = new Disconnections();
            var schema = new NetEntriesSchemaBuilder();
            schema.DeclareResult<int>(NetworkDelivery.Reliable);
            _schema = schema.Build();
            var networkObject = _objects.Create("Net object").AddComponent<NetworkObject>();
            var entries = new NetEntries(_schema, new NetSendGuardFactoryServer(new Ownership(), networkObject));
            var guards = new NetReceiveGuardFactoryServer(_lifetime.Lifetime, new OwnerRegistry(), _clients, new PlayerDrivers(), new NetReceiveConfig(), networkObject);
            _inbox = new NetInboxServer(entries, guards, _clients);
            new NetSchemaHashChecker(_lifetime.Lifetime, _schema, entries, _disconnections);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void ClientWithTheSameSchemaStays()
        {
            _clients.Connect(7);

            Receive(7, _schema.Hash);

            Assert.That(_disconnections.Clients, Is.Empty);
        }

        [Test]
        public void ClientWithAnotherSchemaIsDisconnected()
        {
            _clients.Connect(7);
            _clients.Connect(8);
            LogAssert.Expect(LogType.Exception, new Regex("differs from the server"));

            Receive(8, _schema.Hash + 1);

            Assert.That(_disconnections.Clients, Is.EqualTo(new[] { 8UL }));
        }

        private void Receive(ulong senderClientId, uint hash) =>
            _inbox.Receive(senderClientId, new NetworkMessageHeader { NetworkObjectId = 1, VarId = 0 }, new ValueReader<uint>(hash, senderClientId));

        private sealed class Disconnections : IClientDisconnector
        {
            public List<ulong> Clients { get; } = new();

            public void Disconnect(ulong clientId, string reason) =>
                Clients.Add(clientId);
        }
    }
}
