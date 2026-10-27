using Bw.Entities.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network
{
    public sealed class ControllingClientsTests
    {
        private LifetimeDefinition _lifetime;
        private ControllingClients _controlling;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _controlling = new ControllingClients();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void AddedClientIsListed()
        {
            var client = new Client(new ConnectionRequest("Саня", 3, System.Guid.NewGuid(), System.Guid.Empty));

            _controlling.Add(_lifetime.Lifetime, client);

            Assert.That(_controlling, Is.EqualTo(new IClient[] { client }));
        }

        [Test]
        public void ClientIsUnlistedWhenItsControlEnds()
        {
            var stay = new Client(new ConnectionRequest("Саня", 3, System.Guid.NewGuid(), System.Guid.Empty));
            var leave = new Client(new ConnectionRequest("Сева", 4, System.Guid.NewGuid(), System.Guid.Empty));
            var control = new LifetimeDefinition();
            _controlling.Add(_lifetime.Lifetime, stay);
            _controlling.Add(control.Lifetime, leave);

            control.Terminate();

            Assert.That(_controlling, Is.EqualTo(new IClient[] { stay }));
        }

        [Test]
        public void ListingIsAnnounced()
        {
            var client = new Client(new ConnectionRequest("Саня", 3, System.Guid.NewGuid(), System.Guid.Empty));
            var control = new LifetimeDefinition();
            var changes = 0;
            _controlling.Change.Advise(_lifetime.Lifetime, _ => changes++);

            _controlling.Add(control.Lifetime, client);
            control.Terminate();

            Assert.That(changes, Is.EqualTo(2));
        }
    }
}
