using System;
using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Doubles;
using Bw.UseCases;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Variables
{
    public sealed class NetEntriesServerTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private NetworkObject _networkObject;
        private Ownership _ownership;
        private ClientsById _clients;
        private NetRequestDeclaration<int> _request;
        private NetResultDeclaration<int> _result;
        private NetPropertyDeclaration<int> _serverProperty;
        private NetEntries _entries;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _networkObject = _objects.Create("Net object").AddComponent<NetworkObject>();
            _ownership = new Ownership();
            _clients = new ClientsById(_lifetime.Lifetime);
            var schema = new NetEntriesSchemaBuilder();
            _request = schema.DeclareRequest<int>(NetworkDelivery.Reliable);
            _result = schema.DeclareResult<int>(NetworkDelivery.Reliable);
            _serverProperty = schema.DeclareProperty(3, NetworkDelivery.Reliable, NetworkPermissions.Server);
            _entries = EntriesOf(schema.Build());
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void RequestIsNeverSentByTheServer()
        {
            _ownership.SetMine(_lifetime.Lifetime);
            var sent = new List<int>();
            _entries.RequestSender(_request).Sent.Advise(_lifetime.Lifetime, sent.Add);

            Assert.Throws<InvalidOperationException>(() => _entries.RequestSender(_request).Send(1));
            Assert.That(sent, Is.Empty);
        }

        [Test]
        public void ResultIsBroadcastByTheServer()
        {
            Assert.DoesNotThrow(() => _entries.ResultSender(_result).Broadcast(1));
        }

        [Test]
        public void ResultIsSentToOneClientByTheServer()
        {
            var recipient = _clients.Connect(7);

            Assert.DoesNotThrow(() => _entries.ResultSender(_result).SendTo(recipient, 1));
        }

        [Test]
        public void ResultSentToNobodyThrows()
        {
            Assert.Throws<ArgumentNullException>(() => _entries.ResultSender(_result).SendTo(null, 1));
        }

        [Test]
        public void ServerPropertyIsWrittenByTheServer()
        {
            var property = _entries.Property(_serverProperty);

            property.Value = 4;

            Assert.That(property.Value, Is.EqualTo(4));
        }

        [Test]
        public void OwnerPropertyIsNotWrittenByTheServerWhileNotMine()
        {
            var property = PropertyWith(NetworkPermissions.Owner);

            Assert.Throws<InvalidOperationException>(() => property.Value = 4);
            Assert.That(property.Value, Is.EqualTo(0));
        }

        [Test]
        public void OwnerPropertyIsWrittenByTheServerWhileMine()
        {
            var property = PropertyWith(NetworkPermissions.Owner);
            _ownership.SetMine(_lifetime.Lifetime);

            property.Value = 4;

            Assert.That(property.Value, Is.EqualTo(4));
        }

        [Test]
        public void ClientPropertyIsNeverWrittenByTheServer()
        {
            var property = PropertyWith(NetworkPermissions.Client);
            _ownership.SetMine(_lifetime.Lifetime);

            Assert.Throws<InvalidOperationException>(() => property.Value = 4);
            Assert.That(property.Value, Is.EqualTo(0));
        }

        [Test]
        public void EveryonePropertyIsWrittenByTheServer()
        {
            var property = PropertyWith(NetworkPermissions.Everyone);

            property.Value = 4;

            Assert.That(property.Value, Is.EqualTo(4));
        }

        [Test]
        public void DeclarationOfAnotherSchemaIsRefused()
        {
            var other = new NetEntriesSchemaBuilder();
            var property = other.DeclareProperty(0, NetworkDelivery.Reliable, NetworkPermissions.Server);
            other.Build();

            Assert.Throws<InvalidOperationException>(() => _entries.Property(property));
        }

        [Test]
        public void PropertyWithUnknownPermissionsIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PropertyWith((NetworkPermissions)0));
        }

        private INetProperty<int> PropertyWith(NetworkPermissions permissions)
        {
            var schema = new NetEntriesSchemaBuilder();
            var declaration = schema.DeclareProperty(0, NetworkDelivery.Reliable, permissions);
            return EntriesOf(schema.Build()).Property(declaration);
        }

        private NetEntries EntriesOf(NetEntriesSchema schema) =>
            new(schema, new NetSendGuardFactoryServer(_ownership, _networkObject));
    }
}
