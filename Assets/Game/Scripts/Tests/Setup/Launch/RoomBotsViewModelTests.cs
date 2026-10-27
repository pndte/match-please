using System.Collections.Generic;
using Bw.CoreAdapters.Bots;
using Bw.UseCases.Bots;
using NUnit.Framework;
using Setup.Launch.Menu;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Tests.Setup.Launch
{
    public sealed class RoomBotsViewModelTests
    {
        private readonly List<Object> _created = new();
        private BotPresetCatalog _presets;
        private RoomBotsConfig _room;
        private RoomBotsViewModel _bots;

        [SetUp]
        public void SetUp()
        {
            _presets = Created<BotPresetCatalog>();
            _presets.Difficulties = new[] { Keyed<BotDifficultyConfig>("Light", 11), Keyed<BotDifficultyConfig>("Medium", 12), Keyed<BotDifficultyConfig>("Hard", 10) };
            _presets.Personalities = new[] { Keyed<BotPersonality>("Hunter", 17) };
            _presets.DefaultDifficulty = _presets.Difficulties[1];
            var looks = Created<BotPresetLooksConfig>();
            looks.Difficulties = new[]
            {
                new DifficultyLook { Difficulty = _presets.Difficulties[0], DisplayName = "Лёгкий" },
                new DifficultyLook { Difficulty = _presets.Difficulties[1], DisplayName = "Средний" },
                new DifficultyLook { Difficulty = _presets.Difficulties[2], DisplayName = "Сложный" },
            };
            looks.Personalities = new[] { new PersonalityLook { Personality = _presets.Personalities[0], DisplayName = "Охотник" } };
            _room = Created<RoomBotsConfig>();
            _room.MaxBots = 2;
            _room.Names = new[] { "Пельмень", "Батон" };
            _bots = new RoomBotsViewModel(_presets, looks, _room);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var created in _created)
                Object.DestroyImmediate(created);

            _created.Clear();
        }

        [Test]
        public void NewBotIsMediumWithANameFromTheList()
        {
            _bots.Add();

            Assert.That(_bots.Rows.Value.Count, Is.EqualTo(1));
            Assert.That(_room.Names, Does.Contain(_bots.Rows.Value[0].Nickname));
            Assert.That(_bots.Rows.Value[0].Difficulty, Is.EqualTo("Средний"));
            Assert.That(_bots.RoomBots[0].DifficultyKey, Is.EqualTo(12));
            Assert.That(_bots.RoomBots[0].PersonalityKey, Is.EqualTo(17));
        }

        [Test]
        public void BotsGetNamesOfTheirOwn()
        {
            _bots.Add();
            _bots.Add();

            Assert.That(_bots.Rows.Value[0].Nickname, Is.Not.EqualTo(_bots.Rows.Value[1].Nickname));
        }

        [Test]
        public void RoomIsFullAtItsLimit()
        {
            _bots.Add();
            Assert.That(_bots.Full.Value, Is.False);

            _bots.Add();

            Assert.That(_bots.Full.Value, Is.True);
            Assert.Throws<System.InvalidOperationException>(_bots.Add);
        }

        [Test]
        public void DifficultyCyclesThroughTheCatalog()
        {
            _bots.Add();

            _bots.CycleDifficulty(0);
            Assert.That(_bots.Rows.Value[0].Difficulty, Is.EqualTo("Сложный"));

            _bots.CycleDifficulty(0);
            Assert.That(_bots.Rows.Value[0].Difficulty, Is.EqualTo("Лёгкий"));
            Assert.That(_bots.RoomBots[0].DifficultyKey, Is.EqualTo(11));
        }

        [Test]
        public void RemovedBotLeavesTheList()
        {
            _bots.Add();
            _bots.Add();

            _bots.Remove(0);

            Assert.That(_bots.RoomBots.Count, Is.EqualTo(1));
            Assert.That(_bots.Full.Value, Is.False);
        }

        private T Created<T>() where T : ScriptableObject
        {
            var created = ScriptableObject.CreateInstance<T>();
            _created.Add(created);
            return created;
        }

        private T Keyed<T>(string name, int key) where T : Bw.Entities.Content.ContentAsset
        {
            var created = Created<T>();
            created.name = name;
            var serialized = new UnityEditor.SerializedObject(created);
            serialized.FindProperty(Bw.Entities.Content.ContentAsset.KeyField).intValue = key;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return created;
        }
    }
}
