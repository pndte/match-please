using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network
{
    public sealed class ClientTests
    {
        [Test]
        public void ClientKeepsItsIdAndRequest()
        {
            var client = new Client(7, new ConnectionRequest("Саня", 3));

            Assert.That(client.Id, Is.EqualTo(7UL));
            Assert.That(client.Request.Nickname, Is.EqualTo("Саня"));
            Assert.That(client.Request.CharacterKey, Is.EqualTo(3));
        }

        [Test]
        public void NewClientIsLoading()
        {
            var client = new Client(7, new ConnectionRequest("Саня", 3));

            Assert.That(client.ConnectionState.Value, Is.EqualTo(ClientConnectionState.Loading));
        }

        [Test]
        public void StateChangesAreAnnounced()
        {
            var lifetime = new LifetimeDefinition();
            var client = new Client(7, new ConnectionRequest("Саня", 3));
            var seen = new List<ClientConnectionState>();
            client.ConnectionState.Advise(lifetime.Lifetime, seen.Add);

            client.ChangeState(ClientConnectionState.Connected);
            client.ChangeState(ClientConnectionState.Loading);

            Assert.That(seen, Is.EqualTo(new[] { ClientConnectionState.Loading, ClientConnectionState.Connected, ClientConnectionState.Loading }));
            lifetime.Terminate();
        }

        [Test]
        public void ClientIsNamedByItsId()
        {
            Assert.That(new Client(42, new ConnectionRequest("Сева", 4)).ToString(), Is.EqualTo("42"));
        }

        [Test]
        public void ConnectionStatesKeepTheirWireValues()
        {
            Assert.That((int)ClientConnectionState.Loading, Is.EqualTo(0));
            Assert.That((int)ClientConnectionState.Connected, Is.EqualTo(1));
        }
    }
}
