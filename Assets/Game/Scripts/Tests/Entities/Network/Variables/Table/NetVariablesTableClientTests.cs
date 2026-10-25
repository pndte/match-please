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
    public sealed class NetVariablesTableClientTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private NetworkObject _networkObject;
        private Ownership _ownership;
        private CountingSenders _senders;
        private NetRequestDeclaration<int> _request;
        private NetResultDeclaration<int> _result;
        private NetPropertyDeclaration<int> _serverProperty;
        private NetPropertyDeclaration<int> _everyoneProperty;
        private NetPropertyDeclaration<int> _otherEveryoneProperty;
        private NetVariablesTableClient _table;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _networkObject = _objects.Create("Net object").AddComponent<NetworkObject>();
            _ownership = new Ownership();
            _senders = new CountingSenders();
            var schema = new NetEntriesSchemaBuilder();
            _request = schema.DeclareRequest<int>(NetworkDelivery.Reliable);
            _result = schema.DeclareResult<int>(NetworkDelivery.Reliable);
            _serverProperty = schema.DeclareProperty(3, NetworkDelivery.Reliable, NetworkPermissions.Server);
            _everyoneProperty = schema.DeclareProperty(0, NetworkDelivery.Reliable, NetworkPermissions.Everyone);
            _otherEveryoneProperty = schema.DeclareProperty(0, NetworkDelivery.Reliable, NetworkPermissions.Everyone);
            _table = new NetVariablesTableClient(_lifetime.Lifetime, _networkObject, schema.Build(), _senders, _ownership);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void RequestIsSentWhileTheObjectIsMine()
        {
            _ownership.SetMine(_lifetime.Lifetime);
            var sent = new List<int>();
            _table.Sender(_request).Sent.Advise(_lifetime.Lifetime, sent.Add);

            _table.Sender(_request).Send(5);

            Assert.That(sent, Is.EqualTo(new[] { 5 }));
        }

        [Test]
        public void RequestOfAnObjectNotMineThrows()
        {
            var sent = new List<int>();
            _table.Sender(_request).Sent.Advise(_lifetime.Lifetime, sent.Add);

            Assert.Throws<InvalidOperationException>(() => _table.Sender(_request).Send(5));
            Assert.That(sent, Is.Empty);
        }

        [Test]
        public void RequestThrowsOnceTheObjectIsNoLongerMine()
        {
            var mine = new LifetimeDefinition();
            _ownership.SetMine(mine.Lifetime);
            _table.Sender(_request).Send(1);

            mine.Terminate();

            Assert.Throws<InvalidOperationException>(() => _table.Sender(_request).Send(2));
        }

        [Test]
        public void ResultIsNeverBroadcastByAClient()
        {
            _ownership.SetMine(_lifetime.Lifetime);

            Assert.Throws<InvalidOperationException>(() => _table.Sender(_result).Broadcast(1));
        }

        [Test]
        public void ResultIsNeverSentToAClientByAClient()
        {
            _ownership.SetMine(_lifetime.Lifetime);
            var recipient = new Client(7, new ConnectionRequest("Сева", 4));

            Assert.Throws<InvalidOperationException>(() => _table.Sender(_result).SendTo(recipient, 1));
        }

        [Test]
        public void PropertyStartsAtItsDeclaredValue()
        {
            Assert.That(_table.Get(_serverProperty).Value, Is.EqualTo(3));
        }

        [Test]
        public void ServerPropertyIsNeverWrittenByAClient()
        {
            _ownership.SetMine(_lifetime.Lifetime);
            var property = _table.Get(_serverProperty);

            Assert.Throws<InvalidOperationException>(() => property.Value = 4);
            Assert.That(property.Value, Is.EqualTo(3));
        }

        [TestCase(NetworkPermissions.Owner)]
        [TestCase(NetworkPermissions.Client)]
        public void OwnedPropertyIsNotWrittenWhileNotMine(NetworkPermissions permissions)
        {
            var property = PropertyWith(permissions);

            Assert.Throws<InvalidOperationException>(() => property.Value = 4);
            Assert.That(property.Value, Is.EqualTo(0));
        }

        [TestCase(NetworkPermissions.Owner)]
        [TestCase(NetworkPermissions.Client)]
        public void OwnedPropertyIsWrittenWhileMine(NetworkPermissions permissions)
        {
            var property = PropertyWith(permissions);
            _ownership.SetMine(_lifetime.Lifetime);
            var seen = new List<int>();
            property.Change.Advise(_lifetime.Lifetime, seen.Add);

            property.Value = 4;

            Assert.That(property.Value, Is.EqualTo(4));
            Assert.That(seen, Is.EqualTo(new[] { 4 }));
        }

        [Test]
        public void PropertyIsObservedFromItsCurrentValue()
        {
            var property = _table.Get(_everyoneProperty);
            var seen = new List<int>();
            property.Advise(_lifetime.Lifetime, seen.Add);

            property.Value = 9;

            Assert.That(seen, Is.EqualTo(new[] { 0, 9 }));
        }

        [Test]
        public void EveryonePropertyIsWrittenWithoutOwnership()
        {
            var property = _table.Get(_everyoneProperty);

            property.Value = 9;

            Assert.That(property.Value, Is.EqualTo(9));
        }

        [Test]
        public void DeclarationsOfTheSameTypeAreSeparateEntries()
        {
            _table.Get(_everyoneProperty).Value = 9;

            Assert.That(_table.Get(_otherEveryoneProperty).Value, Is.EqualTo(0));
        }

        [Test]
        public void DeclarationOfAnotherSchemaIsRefused()
        {
            var other = new NetEntriesSchemaBuilder();
            var request = other.DeclareRequest<int>(NetworkDelivery.Reliable);
            var result = other.DeclareResult<int>(NetworkDelivery.Reliable);
            var property = other.DeclareProperty(0, NetworkDelivery.Reliable, NetworkPermissions.Everyone);
            other.Build();

            Assert.Throws<InvalidOperationException>(() => _table.Sender(request));
            Assert.Throws<InvalidOperationException>(() => _table.Receiver(request));
            Assert.Throws<InvalidOperationException>(() => _table.Sender(result));
            Assert.Throws<InvalidOperationException>(() => _table.Receiver(result));
            Assert.Throws<InvalidOperationException>(() => _table.Get(property));
        }

        [Test]
        public void PropertyWithUnknownPermissionsIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PropertyWith((NetworkPermissions)0));
        }

        [Test]
        public void UnspawnedObjectSendsNothing()
        {
            _ownership.SetMine(_lifetime.Lifetime);

            _table.Sender(_request).Send(5);
            _table.Get(_everyoneProperty).Value = 9;

            Assert.That(_senders.Sent, Is.EqualTo(0));
        }

        private INetProperty<int> PropertyWith(NetworkPermissions permissions)
        {
            var schema = new NetEntriesSchemaBuilder();
            var declaration = schema.DeclareProperty(0, NetworkDelivery.Reliable, permissions);
            var table = new NetVariablesTableClient(_lifetime.Lifetime, _networkObject, schema.Build(), _senders, _ownership);
            return table.Get(declaration);
        }
    }
}
