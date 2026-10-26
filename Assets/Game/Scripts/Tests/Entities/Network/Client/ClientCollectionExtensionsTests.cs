using System;
using Bw.Entities.Network;
using Bw.Tests.Entities.Network.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network
{
    public sealed class ClientCollectionExtensionsTests
    {
        [Test]
        public void ConnectedClientHasItsId()
        {
            var lifetime = new LifetimeDefinition();
            var clients = new ClientsById(lifetime.Lifetime);
            var client = clients.Connect(7);

            Assert.That(clients.IdOf(client), Is.EqualTo(7UL));
            lifetime.Terminate();
        }

        [Test]
        public void ClientOutsideTheCollectionHasNoId()
        {
            var lifetime = new LifetimeDefinition();
            var clients = new ClientsById(lifetime.Lifetime);

            Assert.Throws<InvalidOperationException>(() => clients.IdOf(new Client(new ConnectionRequest("Чужой", 1, System.Guid.NewGuid()))));
            lifetime.Terminate();
        }
    }
}
