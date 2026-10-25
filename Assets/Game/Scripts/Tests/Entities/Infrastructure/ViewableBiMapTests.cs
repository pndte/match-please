using System;
using System.Collections.Generic;
using Bw.Entities.Infrastructure;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Infrastructure
{
    public sealed class ViewableBiMapTests
    {
        private LifetimeDefinition _lifetime;
        private ViewableBiMap<int, string> _map;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _map = new ViewableBiMap<int, string>(_lifetime.Lifetime);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void AddedPairIsFoundFromBothSides()
        {
            _map.Add(1, "one");

            Assert.That(_map.TryGetRight(1, out var right), Is.True);
            Assert.That(right, Is.EqualTo("one"));
            Assert.That(_map.TryGetLeft("one", out var left), Is.True);
            Assert.That(left, Is.EqualTo(1));
            Assert.That(_map[1], Is.EqualTo("one"));
            Assert.That(_map.Inverse["one"], Is.EqualTo(1));
            Assert.That(_map.Count, Is.EqualTo(1));
            Assert.That(_map.Inverse.Count, Is.EqualTo(1));
        }

        [Test]
        public void MissingKeysAreFoundOnNeitherSide()
        {
            _map.Add(1, "one");

            Assert.That(_map.TryGetRight(2, out _), Is.False);
            Assert.That(_map.TryGetLeft("two", out _), Is.False);
            Assert.That(_map.ContainsKey(2), Is.False);
            Assert.That(_map.Inverse.ContainsKey("two"), Is.False);
        }

        [Test]
        public void SecondPairForALeftIsRefused()
        {
            _map.Add(1, "one");

            Assert.Throws<ArgumentException>(() => _map.Add(1, "uno"));
            Assert.That(_map[1], Is.EqualTo("one"));
            Assert.That(_map.Inverse.ContainsKey("uno"), Is.False);
        }

        [Test]
        public void SecondPairForARightIsRefused()
        {
            _map.Add(1, "one");

            Assert.Throws<ArgumentException>(() => _map.Add(2, "one"));
            Assert.That(_map.ContainsKey(2), Is.False);
            Assert.That(_map.Inverse["one"], Is.EqualTo(1));
        }

        [Test]
        public void RemovingTheLeftRemovesThePair()
        {
            _map.Add(1, "one");

            Assert.That(_map.Remove(1), Is.True);
            Assert.That(_map.Count, Is.EqualTo(0));
            Assert.That(_map.Inverse.ContainsKey("one"), Is.False);
        }

        [Test]
        public void RemovingTheRightRemovesThePair()
        {
            _map.Add(1, "one");

            Assert.That(_map.RemoveRight("one"), Is.True);
            Assert.That(_map.ContainsKey(1), Is.False);
            Assert.That(_map.Inverse.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemovingUnknownKeysChangesNothing()
        {
            _map.Add(1, "one");

            Assert.That(_map.Remove(2), Is.False);
            Assert.That(_map.RemoveRight("two"), Is.False);
            Assert.That(_map.Count, Is.EqualTo(1));
        }

        [Test]
        public void PairIsRemovedOnlyWhenBothSidesMatch()
        {
            _map.Add(1, "one");

            Assert.That(_map.Remove(new KeyValuePair<int, string>(1, "uno")), Is.False);
            Assert.That(_map.Inverse.Remove(new KeyValuePair<string, int>("one", 2)), Is.False);
            Assert.That(_map.Count, Is.EqualTo(1));
            Assert.That(_map.Remove(new KeyValuePair<int, string>(1, "one")), Is.True);
            Assert.That(_map.Inverse.Count, Is.EqualTo(0));
        }

        [Test]
        public void ClearEmptiesBothSides()
        {
            _map.Add(1, "one");
            _map.Add(2, "two");

            _map.Clear();

            Assert.That(_map.Count, Is.EqualTo(0));
            Assert.That(_map.Inverse.Count, Is.EqualTo(0));
        }

        [Test]
        public void RebindingALeftMovesItToTheNewRight()
        {
            _map.Add(1, "one");

            _map.RebindLeft(1, "uno");

            Assert.That(_map[1], Is.EqualTo("uno"));
            Assert.That(_map.Inverse["uno"], Is.EqualTo(1));
            Assert.That(_map.Inverse.ContainsKey("one"), Is.False);
        }

        [Test]
        public void RebindingAFreeLeftAddsThePair()
        {
            _map.RebindLeft(1, "one");

            Assert.That(_map.Inverse["one"], Is.EqualTo(1));
        }

        [Test]
        public void RebindingALeftToItsOwnRightChangesNothing()
        {
            _map.Add(1, "one");
            var events = Events();

            _map.RebindLeft(1, "one");

            Assert.That(events, Is.Empty);
        }

        [Test]
        public void RebindingALeftToARightTakenByAnotherIsRefused()
        {
            _map.Add(1, "one");
            _map.Add(2, "two");

            Assert.Throws<InvalidOperationException>(() => _map.RebindLeft(1, "two"));
            Assert.That(_map[1], Is.EqualTo("one"));
            Assert.That(_map[2], Is.EqualTo("two"));
        }

        [Test]
        public void IndexerRebindsTheLeft()
        {
            _map.Add(1, "one");

            _map[1] = "uno";

            Assert.That(_map.Inverse["uno"], Is.EqualTo(1));
            Assert.That(_map.Inverse.ContainsKey("one"), Is.False);
        }

        [Test]
        public void RebindingARightMovesItToTheNewLeft()
        {
            _map.Add(1, "one");

            _map.RebindRight("one", 2);

            Assert.That(_map[2], Is.EqualTo("one"));
            Assert.That(_map.ContainsKey(1), Is.False);
            Assert.That(_map.Inverse["one"], Is.EqualTo(2));
        }

        [Test]
        public void RebindingARightToItsOwnLeftChangesNothing()
        {
            _map.Add(1, "one");
            var events = Events();

            _map.RebindRight("one", 1);

            Assert.That(events, Is.Empty);
        }

        [Test]
        public void RebindingARightToALeftTakenByAnotherIsRefused()
        {
            _map.Add(1, "one");
            _map.Add(2, "two");

            Assert.Throws<InvalidOperationException>(() => _map.RebindRight("one", 2));
            Assert.That(_map[1], Is.EqualTo("one"));
            Assert.That(_map[2], Is.EqualTo("two"));
        }

        [Test]
        public void InverseIndexerRebindsTheRight()
        {
            _map.Add(1, "one");

            _map.Inverse["one"] = 2;

            Assert.That(_map[2], Is.EqualTo("one"));
            Assert.That(_map.ContainsKey(1), Is.False);
        }

        [Test]
        public void InverseAddsAndRemovesPairs()
        {
            _map.Inverse.Add("one", 1);
            _map.Inverse.Add(new KeyValuePair<string, int>("two", 2));

            var removed = _map.Inverse.Remove("one");

            Assert.That(removed, Is.True);
            Assert.That(_map.ContainsKey(1), Is.False);
            Assert.That(_map[2], Is.EqualTo("two"));
        }

        [Test]
        public void InverseClearEmptiesBothSides()
        {
            _map.Add(1, "one");

            _map.Inverse.Clear();

            Assert.That(_map.Count, Is.EqualTo(0));
        }

        [Test]
        public void BothSidesListTheSamePairs()
        {
            _map.Add(1, "one");
            _map.Add(2, "two");

            Assert.That(_map.Keys, Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(_map.Values, Is.EquivalentTo(new[] { "one", "two" }));
            Assert.That(_map.Inverse.Keys, Is.EquivalentTo(new[] { "one", "two" }));
            Assert.That(_map.Inverse.Values, Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(_map.Contains(new KeyValuePair<int, string>(2, "two")), Is.True);
            Assert.That(_map.Inverse.Contains(new KeyValuePair<string, int>("two", 2)), Is.True);
            Assert.That(_map.Inverse.Contains(new KeyValuePair<string, int>("two", 1)), Is.False);
        }

        [Test]
        public void PairsAreCopiedFromBothSides()
        {
            _map.Add(1, "one");
            var forward = new KeyValuePair<int, string>[2];
            var inverse = new KeyValuePair<string, int>[2];

            _map.CopyTo(forward, 1);
            _map.Inverse.CopyTo(inverse, 1);

            Assert.That(forward[1], Is.EqualTo(new KeyValuePair<int, string>(1, "one")));
            Assert.That(inverse[1], Is.EqualTo(new KeyValuePair<string, int>("one", 1)));
        }

        [Test]
        public void BothSidesAreWritable()
        {
            Assert.That(_map.IsReadOnly, Is.False);
            Assert.That(_map.Inverse.IsReadOnly, Is.False);
        }

        [Test]
        public void AdviseReplaysThePairsThenFollowsChanges()
        {
            _map.Add(1, "one");
            var events = new List<string>();

            _map.Advise(_lifetime.Lifetime, change => events.Add($"{change.Kind} {change.Key}"));
            _map.Add(2, "two");
            _map.Remove(1);

            Assert.That(events, Is.EqualTo(new[] { "Add 1", "Add 2", "Remove 1" }));
        }

        [Test]
        public void InverseAdviseReplaysThePairsThenFollowsChanges()
        {
            _map.Add(1, "one");
            var events = new List<string>();

            _map.Inverse.Advise(_lifetime.Lifetime, change => events.Add($"{change.Kind} {change.Key}"));
            _map.Add(2, "two");
            _map.RemoveRight("one");

            Assert.That(events, Is.EqualTo(new[] { "Add one", "Add two", "Remove one" }));
        }

        [Test]
        public void ChangeCarriesOnlyLaterEvents()
        {
            _map.Add(1, "one");
            var forward = new List<int>();
            var inverse = new List<string>();

            _map.Change.Advise(_lifetime.Lifetime, change => forward.Add(change.Key));
            _map.Inverse.Change.Advise(_lifetime.Lifetime, change => inverse.Add(change.Key));
            _map.Add(2, "two");

            Assert.That(forward, Is.EqualTo(new[] { 2 }));
            Assert.That(inverse, Is.EqualTo(new[] { "two" }));
        }

        [Test]
        public void LateViewEndsAnEntryThatExistedBeforeIt()
        {
            _map.Add(1, "one");
            var lifetimes = new List<Lifetime>();
            _map.View(_lifetime.Lifetime, (entryLifetime, _, _) => lifetimes.Add(entryLifetime));

            _map.Remove(1);

            Assert.That(lifetimes, Has.Count.EqualTo(1));
            Assert.That(lifetimes[0].IsAlive, Is.False);
        }

        [Test]
        public void LateInverseViewEndsAnEntryThatExistedBeforeIt()
        {
            _map.Add(1, "one");
            var lifetimes = new List<Lifetime>();
            _map.Inverse.View(_lifetime.Lifetime, (entryLifetime, _, _) => lifetimes.Add(entryLifetime));

            _map.Remove(1);

            Assert.That(lifetimes, Has.Count.EqualTo(1));
            Assert.That(lifetimes[0].IsAlive, Is.False);
        }

        [Test]
        public void AdviseStopsWithItsLifetime()
        {
            var lifetime = _lifetime.Lifetime.CreateNested();
            var events = new List<int>();
            _map.Advise(lifetime.Lifetime, change => events.Add(change.Key));
            lifetime.Terminate();

            _map.Add(1, "one");

            Assert.That(events, Is.Empty);
        }

        private List<string> Events()
        {
            var events = new List<string>();
            _map.Change.Advise(_lifetime.Lifetime, change => events.Add($"{change.Kind} {change.Key}"));
            _map.Inverse.Change.Advise(_lifetime.Lifetime, change => events.Add($"{change.Kind} {change.Key}"));
            return events;
        }
    }
}
