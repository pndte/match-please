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
    public sealed class NetInboxClientTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private NetResultDeclaration<int> _result;
        private NetPropertyDeclaration<int> _serverProperty;
        private NetEntries _entries;
        private INetInbox _inbox;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            var schema = new NetEntriesSchemaBuilder();
            _result = schema.DeclareResult<int>(NetworkDelivery.Reliable);
            _serverProperty = schema.DeclareProperty(3, NetworkDelivery.Reliable, NetworkPermissions.Server);
            _entries = new NetEntries(schema.Build(), new NetSendGuardFactoryClient(new Ownership(), _objects.Create("Net object").AddComponent<NetworkObject>()));
            _inbox = new NetInboxClient(_entries);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void ResultOfTheServerIsHeard()
        {
            var heard = new List<int>();
            _entries.ResultReceiver(_result).Received.Advise(_lifetime.Lifetime, heard.Add);

            Receive(1, 5);

            Assert.That(heard, Is.EqualTo(new[] { 5 }));
        }

        [Test]
        public void PropertyOfTheServerTakesItsValue()
        {
            Receive(2, 8);

            Assert.That(_entries.Property(_serverProperty).Value, Is.EqualTo(8));
        }

        [Test]
        public void UnknownEntryThrows()
        {
            Assert.Throws<InvalidOperationException>(() => Receive(5, 1));
        }

        private void Receive(ushort varId, int value) =>
            _inbox.Receive(NetworkManager.ServerClientId, new NetworkMessageHeader { NetworkObjectId = 1, VarId = varId }, new ValueReader<int>(value, NetworkManager.ServerClientId));
    }
}
