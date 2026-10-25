using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Bubble;
using Bw.Entities.Players;
using Bw.Tests.UseCases.Character.Doubles;
using Bw.UseCases;
using Bw.UseCases.Character.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Character.Network
{
    public sealed class CharacterInputAudienceTests
    {
        private const float RelayRadius = 24f;

        private LifetimeDefinition _lifetime;
        private CharacterRegistry _registry;
        private PlayerDrivers _drivers;
        private FakeCharacter _mover;
        private CharacterInputAudience _audience;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _registry = new CharacterRegistry();
            _drivers = new PlayerDrivers();
            _mover = new FakeCharacter(Vector2.zero);
            var bubble = new PredictionBubbleConfig { EnterRadius = 14f, LeaveRadius = 18f, InputRelayMargin = 6f };
            _audience = new CharacterInputAudience(_mover, _registry, _drivers, bubble);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void ClientWatchingFromWithinTheRelayRadiusHearsTheInputs()
        {
            var watcher = ClientWatching(new Vector2(RelayRadius - 0.5f, 0f), 2UL);

            Assert.That(Collected(), Is.EqualTo(new[] { watcher }));
        }

        [Test]
        public void RelayRadiusIsTheLeaveRadiusWidenedByTheMargin()
        {
            var atTheEdge = ClientWatching(new Vector2(0f, -RelayRadius), 2UL);
            ClientWatching(new Vector2(RelayRadius + 0.01f, 0f), 3UL);

            Assert.That(Collected(), Is.EqualTo(new[] { atTheEdge }));
        }

        [Test]
        public void ClientWatchingFromAfarHearsNothing()
        {
            ClientWatching(new Vector2(40f, 0f), 2UL);

            Assert.That(Collected(), Is.Empty);
        }

        [Test]
        public void MoverIsNotItsOwnAudience()
        {
            var moverClient = new Client(1UL, default);
            var player = new Player("mover");
            _drivers.AddClientDriven(_lifetime.Lifetime, player, moverClient);
            _registry.PlayerByCharacter.Add(_mover, player);

            Assert.That(Collected(), Is.Empty);
        }

        [Test]
        public void BotWatchingNearbyAddsNoClient()
        {
            var player = new Player("bot");
            _drivers.AddServerDriven(_lifetime.Lifetime, player);
            _registry.PlayerByCharacter.Add(new FakeCharacter(new Vector2(3f, 0f)), player);

            Assert.That(Collected(), Is.Empty);
        }

        [Test]
        public void ClientWithTwoWatchingCharactersIsAddedOnce()
        {
            var client = new Client(2UL, default);
            var player = new Player("two characters");
            _drivers.AddClientDriven(_lifetime.Lifetime, player, client);
            _registry.PlayerByCharacter.Add(new FakeCharacter(new Vector2(2f, 0f)), player);
            _registry.PlayerByCharacter.Add(new FakeCharacter(new Vector2(4f, 0f)), player);

            Assert.That(Collected(), Is.EqualTo(new[] { client }));
        }

        [Test]
        public void ClientAlreadyInTheAudienceIsNotAddedAgain()
        {
            var watcher = ClientWatching(new Vector2(5f, 0f), 2UL);
            var clients = new List<IClient> { watcher };

            _audience.Collect(clients);

            Assert.That(clients, Is.EqualTo(new[] { watcher }));
        }

        [Test]
        public void AudienceFollowsWhereEveryoneIsNow()
        {
            var watcherCharacter = new FakeCharacter(new Vector2(5f, 0f));
            var client = new Client(2UL, default);
            var player = new Player("watcher");
            _drivers.AddClientDriven(_lifetime.Lifetime, player, client);
            _registry.PlayerByCharacter.Add(watcherCharacter, player);

            _mover.MoveTo(new Vector2(100f, 0f));
            var whenApart = Collected();
            watcherCharacter.MoveTo(new Vector2(95f, 0f));
            var whenTogether = Collected();

            Assert.That(whenApart, Is.Empty);
            Assert.That(whenTogether, Is.EqualTo(new[] { client }));
        }

        private IClient ClientWatching(Vector2 position, ulong id)
        {
            var client = new Client(id, default);
            var player = new Player($"watcher {id}");
            _drivers.AddClientDriven(_lifetime.Lifetime, player, client);
            _registry.PlayerByCharacter.Add(new FakeCharacter(position), player);
            return client;
        }

        private List<IClient> Collected()
        {
            var clients = new List<IClient>();
            _audience.Collect(clients);
            return clients;
        }
    }
}
