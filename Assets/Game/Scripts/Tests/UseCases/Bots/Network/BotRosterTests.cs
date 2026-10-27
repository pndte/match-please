using System;
using System.Collections.Generic;
using Bw.CoreAdapters.Characters;
using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.UseCases.Bots;
using Bw.UseCases.Bots.Network;
using Bw.UseCases.Character;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Bots.Network
{
    public sealed class BotRosterTests
    {
        private LifetimeDefinition _lifetime;
        private Admissions _admissions;
        private CharacterCatalog _characters;
        private RoomBotsConfig _room;
        private BotPreset _medium;
        private BotPreset _hard;
        private BotRoster _roster;
        private readonly List<Object> _created = new();

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _admissions = new Admissions();
            _characters = Created<CharacterCatalog>();
            _characters.Characters = new[] { new CharacterEntry { Character = Created<CharacterDescription>() } };
            _room = Created<RoomBotsConfig>();
            _room.MaxBots = 2;
            _medium = new BotPreset(Created<BotDifficultyConfig>(), Created<BotPersonality>());
            _hard = new BotPreset(Created<BotDifficultyConfig>(), Created<BotPersonality>());
            _roster = new BotRoster(_lifetime.Lifetime, _admissions, _characters, _room);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            foreach (var created in _created)
                Object.DestroyImmediate(created);

            _created.Clear();
        }

        [Test]
        public void AddedBotIsAdmittedAndListed()
        {
            _roster.Add("Пельмень", _medium);

            var (bot, entry) = Single();
            Assert.That(bot.Nickname, Is.EqualTo("Пельмень"));
            Assert.That(entry.Difficulty, Is.SameAs(_medium.Difficulty));
            Assert.That(entry.Personality, Is.SameAs(_medium.Personality));
            Assert.That(entry.Character, Is.SameAs(_characters.Characters[0].Character));
            Assert.That(_admissions.Drivers[bot], Is.SameAs(PlayerDriver.Server), "a bot is driven by the server");
            Assert.That(_admissions.Lives[bot].IsAlive, Is.True);
        }

        [Test]
        public void RemovedBotLeavesTheRoomAndTheGame()
        {
            _roster.Add("Пельмень", _medium);
            var (bot, _) = Single();

            _roster.Remove(bot);

            Assert.That(_roster.Bots.Count, Is.Zero);
            Assert.That(_admissions.Lives[bot].IsAlive, Is.False);
        }

        [Test]
        public void ChangedBotKeepsItsNameAndCharacter()
        {
            _roster.Add("Пельмень", _medium);
            var (bot, before) = Single();

            _roster.Change(bot, _hard);

            var (same, after) = Single();
            Assert.That(same, Is.SameAs(bot));
            Assert.That(after.Nickname, Is.EqualTo(before.Nickname));
            Assert.That(after.Character, Is.SameAs(before.Character));
            Assert.That(after.Difficulty, Is.SameAs(_hard.Difficulty));
            Assert.That(after.Personality, Is.SameAs(_hard.Personality));
            Assert.That(_admissions.Lives[bot].IsAlive, Is.True, "a new preset keeps the bot in the game");
        }

        [Test]
        public void FullRoomThrows()
        {
            _roster.Add("Пельмень", _medium);
            _roster.Add("Батон", _medium);

            Assert.Throws<InvalidOperationException>(() => _roster.Add("Кефир", _medium));
        }

        [Test]
        public void TakenNameThrows()
        {
            _roster.Add("Пельмень", _medium);

            Assert.Throws<InvalidOperationException>(() => _roster.Add("Пельмень", _hard));
        }

        [Test]
        public void StrangerThrows()
        {
            var stranger = new Player("Саня");

            Assert.Throws<InvalidOperationException>(() => _roster.Remove(stranger));
            Assert.Throws<InvalidOperationException>(() => _roster.Change(stranger, _hard));
        }

        [Test]
        public void RemovedBotFreesItsSeat()
        {
            _roster.Add("Пельмень", _medium);
            _roster.Add("Батон", _medium);
            var (bot, _) = First();
            _roster.Remove(bot);

            Assert.DoesNotThrow(() => _roster.Add("Кефир", _medium));
        }

        private (IPlayer, BotEntry) Single()
        {
            Assert.That(_roster.Bots.Count, Is.EqualTo(1));
            return First();
        }

        private (IPlayer, BotEntry) First()
        {
            foreach (var (bot, entry) in _roster.Bots)
                return (bot, entry);

            throw new InvalidOperationException("The roster is empty.");
        }

        private T Created<T>() where T : ScriptableObject
        {
            var created = ScriptableObject.CreateInstance<T>();
            _created.Add(created);
            return created;
        }

        private sealed class Admissions : IPlayerAdmitter
        {
            public readonly Dictionary<IPlayer, Lifetime> Lives = new();
            public readonly Dictionary<IPlayer, PlayerDriver> Drivers = new();

            public void Admit(
                Lifetime lifetime,
                IPlayer player,
                int characterKey,
                PlayerDriver driver)
            {
                Lives[player] = lifetime;
                Drivers[player] = driver;
            }
        }
    }
}
