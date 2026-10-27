using System;
using System.Collections.Generic;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.Tests.UseCases.Combat.Hands.Doubles;
using Bw.UseCases.Bots;
using Bw.UseCases.Bots.Senses;
using Bw.UseCases.Character;
using Bw.UseCases.Combat.Abstractions.Characters;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Bots.Senses
{
    public sealed class BotSightTests
    {
        private PhysicsTestWorld _world;
        private BotsConfig _bots;
        private BotDifficultyConfig _difficulty;
        private GameObjectByCharacterCollection _characters;
        private IReadonlyCharacter _self;
        private BotSight _sight;

        [SetUp]
        public void SetUp()
        {
            _world = new PhysicsTestWorld();
            _bots = ScriptableObject.CreateInstance<BotsConfig>();
            _difficulty = ScriptableObject.CreateInstance<BotDifficultyConfig>();
            _bots.SightHalfExtents = new Vector2(10f, 5f);
            _difficulty.ReactionTime = 0.09f;
            _difficulty.MemoryTime = 0.19f;
            _characters = new GameObjectByCharacterCollection();
            _self = Place(new Vector2(0f, 1f));
            _sight = new BotSight(_self, _characters, _world.Space, _bots, _difficulty, _world.Clock);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_difficulty);
            Object.DestroyImmediate(_bots);
            _world.Dispose();
        }

        [Test]
        public void EyeIsWhereTheBotStands()
        {
            Assert.That(_sight.Eye, Is.EqualTo(new Vector2(0f, 1f)));
        }

        [Test]
        public void BotNeverSeesItself()
        {
            Assert.That(_sight.SightingsAfter(Array.Empty<Sighting>()), Is.Empty);
        }

        [Test]
        public void CharacterInTheOpenIsSeenButNotYetNoticed()
        {
            var enemy = Place(new Vector2(5f, 1f));

            var sightings = _sight.SightingsAfter(Array.Empty<Sighting>());

            Assert.That(sightings, Has.Count.EqualTo(1));
            Assert.That(sightings[0].Character, Is.SameAs(enemy));
            Assert.That(sightings[0].Position, Is.EqualTo(new Vector2(5f, 1f)));
            Assert.That(sightings[0].Visible, Is.True);
            Assert.That(sightings[0].Noticed, Is.False);
        }

        [Test]
        public void CharacterIsNoticedAfterTheReactionTime()
        {
            Place(new Vector2(5f, 1f));
            IReadOnlyList<Sighting> sightings = Array.Empty<Sighting>();

            for (var look = 0; look < 3; look++)
                sightings = _sight.SightingsAfter(sightings);

            Assert.That(sightings[0].Noticed, Is.True);
        }

        [Test]
        public void InstantReactionNoticesAtFirstSight()
        {
            _difficulty.ReactionTime = 0f;
            Place(new Vector2(5f, 1f));

            var sightings = _sight.SightingsAfter(Array.Empty<Sighting>());

            Assert.That(sightings[0].Noticed, Is.True);
        }

        [TestCase(10.5f, 1f)]
        [TestCase(-10.5f, 1f)]
        [TestCase(0f, 6.5f)]
        [TestCase(0f, -4.5f)]
        public void CharacterOutsideTheFrameIsNotSeen(float x, float y)
        {
            Place(new Vector2(x, y));

            Assert.That(_sight.SightingsAfter(Array.Empty<Sighting>()), Is.Empty, "a bot sees no further than a player's camera frame around it");
        }

        [Test]
        public void CharacterInTheFrameCornerIsSeen()
        {
            Place(new Vector2(9.5f, 5.5f));

            Assert.That(_sight.SightingsAfter(Array.Empty<Sighting>()), Has.Count.EqualTo(1));
        }

        [Test]
        public void WallHidesTheCharacter()
        {
            _world.Static("Wall", new Vector2(2.5f, 1f), new Vector2(1f, 10f));
            Place(new Vector2(5f, 1f));

            Assert.That(_sight.SightingsAfter(Array.Empty<Sighting>()), Is.Empty);
        }

        [Test]
        public void OneWayPlankDoesNotHideTheCharacter()
        {
            _world.StaticPlank("Plank", new Vector2(2.5f, 1f), new Vector2(1f, 0.2f));
            Place(new Vector2(5f, 1f));

            Assert.That(_sight.SightingsAfter(Array.Empty<Sighting>()), Has.Count.EqualTo(1));
        }

        [Test]
        public void DeadCharacterIsNotSeen()
        {
            Place(new Vector2(5f, 1f), new FakeMortal(Bw.UseCases.Mortality.LifeState.Dead));

            Assert.That(_sight.SightingsAfter(Array.Empty<Sighting>()), Is.Empty);
        }

        [Test]
        public void HiddenCharacterIsRememberedWhereItWasLastSeen()
        {
            var enemy = Place(new Vector2(5f, 1f));
            var sightings = _sight.SightingsAfter(Array.Empty<Sighting>());
            _world.Static("Wall", new Vector2(2.5f, 1f), new Vector2(1f, 10f));
            _characters[enemy].transform.position = new Vector2(7f, 1f);

            sightings = _sight.SightingsAfter(sightings);

            Assert.That(sightings, Has.Count.EqualTo(1));
            Assert.That(sightings[0].Visible, Is.False);
            Assert.That(sightings[0].Position, Is.EqualTo(new Vector2(5f, 1f)));
            Assert.That(sightings[0].UnseenTicks, Is.EqualTo(1));
        }

        [Test]
        public void HiddenCharacterIsForgottenAfterTheMemoryTime()
        {
            Place(new Vector2(5f, 1f));
            var sightings = _sight.SightingsAfter(Array.Empty<Sighting>());
            _world.Static("Wall", new Vector2(2.5f, 1f), new Vector2(1f, 10f));
            for (var look = 0; look < 6; look++)
                sightings = _sight.SightingsAfter(sightings);

            Assert.That(sightings, Has.Count.EqualTo(1));

            sightings = _sight.SightingsAfter(_sight.SightingsAfter(sightings));

            Assert.That(sightings, Is.Empty);
        }

        [Test]
        public void RememberedCharacterThatDiesIsForgotten()
        {
            var mortal = new FakeMortal();
            Place(new Vector2(5f, 1f), mortal);
            var sightings = _sight.SightingsAfter(Array.Empty<Sighting>());

            mortal.Die();

            Assert.That(_sight.SightingsAfter(sightings), Is.Empty);
        }

        [Test]
        public void RememberedCharacterThatLeavesTheLevelIsForgotten()
        {
            var enemy = Place(new Vector2(5f, 1f));
            var sightings = _sight.SightingsAfter(Array.Empty<Sighting>());

            _characters.Remove(enemy);

            Assert.That(_sight.SightingsAfter(sightings), Is.Empty);
        }

        [Test]
        public void SightingsAreReturnedAsANewListLeavingTheMemoryAsItWas()
        {
            Place(new Vector2(5f, 1f));
            var first = _sight.SightingsAfter(Array.Empty<Sighting>());

            var second = _sight.SightingsAfter(first);

            Assert.That(first[0].SeenTicks, Is.EqualTo(1));
            Assert.That(second[0].SeenTicks, Is.EqualTo(2));
        }

        private IReadonlyCharacter Place(Vector2 position, FakeMortal mortal = null)
        {
            var character = new Bw.UseCases.Combat.Characters.Character(mortal ?? new FakeMortal(), null, null, null);
            _characters.Add(character, _world.Objects.Create("Character", position));
            return character;
        }
    }
}
