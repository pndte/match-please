using System.Collections.Generic;
using Bw.Entities;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities
{
    public sealed class BwViewableMapTests
    {
        private LifetimeDefinition _lifetime;
        private BwViewableMap<string, int> _map;
        private List<string> _events;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _map = new BwViewableMap<string, int>();
            _events = new List<string>();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void AddAnnouncesTheEntry()
        {
            Follow();

            _map.Add("a", 1);
            _map.Add(new KeyValuePair<string, int>("b", 2));

            Assert.That(_map["a"], Is.EqualTo(1));
            Assert.That(_map["b"], Is.EqualTo(2));
            Assert.That(_events, Is.EqualTo(new[] { "Add a 1", "Add b 2" }));
        }

        [Test]
        public void IndexerAddsAMissingKey()
        {
            Follow();

            _map["a"] = 1;

            Assert.That(_events, Is.EqualTo(new[] { "Add a 1" }));
        }

        [Test]
        public void IndexerAnnouncesAnUpdate()
        {
            _map["a"] = 1;
            Follow();

            _map["a"] = 2;
            _map["a"] = 2;

            Assert.That(_events, Is.EqualTo(new[] { "Add a 1", "Update a 1>2" }));
        }

        [Test]
        public void RemoveAnnouncesTheRemovedValue()
        {
            _map.Add("a", 1);
            Follow();

            var removed = _map.Remove("a");

            Assert.That(removed, Is.True);
            Assert.That(_map.ContainsKey("a"), Is.False);
            Assert.That(_events, Is.EqualTo(new[] { "Add a 1", "Remove a 1" }));
        }

        [Test]
        public void RemovingAMissingKeyChangesNothing()
        {
            Follow();

            Assert.That(_map.Remove("a"), Is.False);
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void PairIsRemovedOnlyWithItsValue()
        {
            _map.Add("a", 1);

            Assert.That(_map.Remove(new KeyValuePair<string, int>("a", 2)), Is.False);
            Assert.That(_map.Remove(new KeyValuePair<string, int>("a", 1)), Is.True);
            Assert.That(_map, Is.Empty);
        }

        [Test]
        public void ClearAnnouncesEveryRemoval()
        {
            _map.Add("a", 1);
            _map.Add("b", 2);
            Follow();
            _events.Clear();

            _map.Clear();

            Assert.That(_map, Is.Empty);
            Assert.That(_events, Is.EquivalentTo(new[] { "Remove a 1", "Remove b 2" }));
        }

        [Test]
        public void ChangeCarriesOnlyLaterEvents()
        {
            _map.Add("a", 1);

            _map.Change.Advise(_lifetime.Lifetime, change => _events.Add(Text(change)));
            _map.Add("b", 2);

            Assert.That(_events, Is.EqualTo(new[] { "Add b 2" }));
        }

        [Test]
        public void AdviseStopsWithItsLifetime()
        {
            var lifetime = _lifetime.Lifetime.CreateNested();
            _map.Advise(lifetime.Lifetime, change => _events.Add(Text(change)));
            lifetime.Terminate();

            _map.Add("a", 1);

            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void MapReadsLikeADictionary()
        {
            _map.Add("a", 1);
            _map.Add("b", 2);
            var copy = new KeyValuePair<string, int>[3];

            _map.CopyTo(copy, 1);

            Assert.That(_map.Count, Is.EqualTo(2));
            Assert.That(_map.TryGetValue("b", out var value), Is.True);
            Assert.That(value, Is.EqualTo(2));
            Assert.That(_map.TryGetValue("x", out _), Is.False);
            Assert.That(_map.Contains(new KeyValuePair<string, int>("a", 1)), Is.True);
            Assert.That(_map.Contains(new KeyValuePair<string, int>("a", 2)), Is.False);
            Assert.That(_map.Keys, Is.EquivalentTo(new[] { "a", "b" }));
            Assert.That(_map.Values, Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(copy, Is.SupersetOf(new[] { new KeyValuePair<string, int>("a", 1), new KeyValuePair<string, int>("b", 2) }));
            Assert.That(_map.IsReadOnly, Is.False);
        }

        [Test]
        public void ReadonlyViewIsTheSameMap()
        {
            IReadonlyViewableMap<string, int> readonlyMap = _map;
            _map.Add("a", 1);

            Assert.That(readonlyMap.Count, Is.EqualTo(1));
            Assert.That(readonlyMap["a"], Is.EqualTo(1));
            Assert.That(readonlyMap.Keys, Is.EquivalentTo(new[] { "a" }));
            Assert.That(readonlyMap.Values, Is.EquivalentTo(new[] { 1 }));
        }

        private void Follow() =>
            _map.Advise(_lifetime.Lifetime, change => _events.Add(Text(change)));

        private static string Text(MapEvent<string, int> change) =>
            change.Kind switch
            {
                AddUpdateRemove.Add => $"Add {change.Key} {change.NewValue}",
                AddUpdateRemove.Update => $"Update {change.Key} {change.OldValue}>{change.NewValue}",
                _ => $"Remove {change.Key} {change.OldValue}"
            };
    }
}
