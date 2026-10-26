using System.Text.RegularExpressions;
using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.UseCases;
using Bw.UseCases.ObjectMeta.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Bw.Tests.UseCases.ObjectMeta.Network
{
    public sealed class ControllerDriversTests
    {
        private LifetimeDefinition _lifetime;
        private ControllerRegistry _registry;
        private PlayerDrivers _drivers;
        private ControlledBy _controlledBy;
        private ControllingClients _clients;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _registry = new ControllerRegistry();
            _drivers = new PlayerDrivers();
            _controlledBy = new ControlledBy();
            _clients = new ControllingClients();
            new ControllerDrivers(_lifetime.Lifetime, _registry, _drivers, _controlledBy, _clients);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void UserDrivenByAClientMakesThatClientAController()
        {
            var player = new Player("Саня");
            var client = new Client(new ConnectionRequest("Саня", 0));
            _drivers.AddClientDriven(_lifetime.Lifetime, player, client);

            _registry.Set(_lifetime.Lifetime, player);

            Assert.That(_clients, Is.EqualTo(new[] { client }));
            Assert.That(_controlledBy.Me.Value, Is.False);
        }

        [Test]
        public void UserDrivenByTheServerMakesTheObjectMine()
        {
            var bot = new Player("Сева");
            _drivers.AddServerDriven(_lifetime.Lifetime, bot);

            _registry.Set(_lifetime.Lifetime, bot);

            Assert.That(_controlledBy.Me.Value, Is.True);
            Assert.That(_clients, Is.Empty);
        }

        [Test]
        public void ControllerLeavesWithTheUse()
        {
            var player = new Player("Саня");
            var client = new Client(new ConnectionRequest("Саня", 0));
            _drivers.AddClientDriven(_lifetime.Lifetime, player, client);
            var use = new LifetimeDefinition();
            _registry.Set(use.Lifetime, player);

            use.Terminate();

            Assert.That(_clients, Is.Empty);
        }

        [Test]
        public void ServerControlEndsWithTheUse()
        {
            var bot = new Player("Сева");
            _drivers.AddServerDriven(_lifetime.Lifetime, bot);
            var use = new LifetimeDefinition();
            _registry.Set(use.Lifetime, bot);

            use.Terminate();

            Assert.That(_controlledBy.Me.Value, Is.False);
        }

        [Test]
        public void EveryUserBringsItsOwnController()
        {
            var first = new Client(new ConnectionRequest("Саня", 0));
            var second = new Client(new ConnectionRequest("Матвей", 2));
            var sanya = new Player("Саня");
            var matvey = new Player("Матвей");
            _drivers.AddClientDriven(_lifetime.Lifetime, sanya, first);
            _drivers.AddClientDriven(_lifetime.Lifetime, matvey, second);

            _registry.Set(_lifetime.Lifetime, sanya);
            _registry.Set(_lifetime.Lifetime, matvey);

            Assert.That(_clients, Is.EquivalentTo(new[] { first, second }));
        }

        [Test]
        public void UserWithoutADriverIsReported()
        {
            LogAssert.Expect(LogType.Exception, new Regex("has no driver"));

            _registry.Set(_lifetime.Lifetime, new Player("Гошак"));

            Assert.That(_clients, Is.Empty);
            Assert.That(_controlledBy.Me.Value, Is.False);
        }
    }
}
