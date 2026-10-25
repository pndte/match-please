using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Extensions
{
    public sealed class ViewablePropertyExtensionsTests
    {
        private LifetimeDefinition _lifetime;
        private List<string> _seen;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _seen = new List<string>();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void WhenLessOrEqualsFiresForValuesUpToTheBound()
        {
            var property = new ViewableProperty<int>(5);
            property.WhenLessOrEquals(_lifetime.Lifetime, 3, () => _seen.Add(property.Value.ToString()));

            property.Value = 4;
            property.Value = 3;
            property.Value = 1;
            property.Value = 7;

            Assert.That(_seen, Is.EqualTo(new[] { "3", "1" }));
        }

        [Test]
        public void WhenLessOrEqualsFiresAtOnceWhenTheValueIsAlreadyLow()
        {
            var property = new ViewableProperty<int>(2);

            property.WhenLessOrEquals(_lifetime.Lifetime, 3, () => _seen.Add("low"));

            Assert.That(_seen, Is.EqualTo(new[] { "low" }));
        }

        [Test]
        public void WhenLessOrEqualsStopsWithItsLifetime()
        {
            var property = new ViewableProperty<int>(5);
            var lifetime = _lifetime.Lifetime.CreateNested();
            property.WhenLessOrEquals(lifetime.Lifetime, 3, () => _seen.Add("low"));
            lifetime.Terminate();

            property.Value = 0;

            Assert.That(_seen, Is.Empty);
        }

        [Test]
        public void AdviseTrueFiresOnlyForTrue()
        {
            var property = new ViewableProperty<bool>(true);
            property.AdviseTrue(_lifetime.Lifetime, () => _seen.Add("true"));

            property.Value = false;
            property.Value = true;

            Assert.That(_seen, Is.EqualTo(new[] { "true", "true" }));
        }

        [Test]
        public void AdviseTrueStopsWithItsLifetime()
        {
            var property = new ViewableProperty<bool>(false);
            var lifetime = _lifetime.Lifetime.CreateNested();
            property.AdviseTrue(lifetime.Lifetime, () => _seen.Add("true"));
            lifetime.Terminate();

            property.Value = true;

            Assert.That(_seen, Is.Empty);
        }

        [Test]
        public void SelectFollowsTheSourceThroughTheSelector()
        {
            var property = new ViewableProperty<int>(2);
            var selected = property.Select(_lifetime.Lifetime, value => value * 10);
            var start = selected.Value;

            property.Value = 3;

            Assert.That(start, Is.EqualTo(20));
            Assert.That(selected.Value, Is.EqualTo(30));
        }

        [Test]
        public void SelectStopsFollowingWithItsLifetime()
        {
            var property = new ViewableProperty<int>(2);
            var lifetime = _lifetime.Lifetime.CreateNested();
            var selected = property.Select(lifetime.Lifetime, value => value * 10);
            lifetime.Terminate();

            property.Value = 3;

            Assert.That(selected.Value, Is.EqualTo(20));
        }

        [Test]
        public void SelectedValueFiresOnlyWhenTheSelectionChanges()
        {
            var property = new ViewableProperty<int>(2);
            var selected = property.Select(_lifetime.Lifetime, value => value / 10);
            selected.Advise(_lifetime.Lifetime, value => _seen.Add(value.ToString()));

            property.Value = 5;
            property.Value = 12;

            Assert.That(_seen, Is.EqualTo(new[] { "0", "1" }));
        }

        [Test]
        public void AdviseAddRemoveSplitsListEventsIntoAddsAndRemoves()
        {
            var list = new BwViewableList<string> { "a" };
            list.AdviseAddRemove(_lifetime.Lifetime, (kind, item) => _seen.Add($"{kind} {item}"));

            list.Add("b");
            list[0] = "c";
            list.Remove("b");

            Assert.That(_seen, Is.EqualTo(new[] { "Add a", "Add b", "Remove a", "Add c", "Remove b" }));
        }

        [Test]
        public void AdviseAddRemoveStopsWithItsLifetime()
        {
            var list = new BwViewableList<string>();
            var lifetime = _lifetime.Lifetime.CreateNested();
            list.AdviseAddRemove(lifetime.Lifetime, (kind, item) => _seen.Add($"{kind} {item}"));
            lifetime.Terminate();

            list.Add("a");

            Assert.That(_seen, Is.Empty);
        }

        [Test]
        public void ListSourceViewGivesEveryItemALifetimeUntilItIsRemoved()
        {
            var list = new BwViewableList<string>();
            var lifetimes = new Dictionary<string, Lifetime>();
            Readonly(list).View(_lifetime.Lifetime, (itemLifetime, item) => lifetimes.Add(item, itemLifetime));
            list.Add("a");
            list.Add("b");

            list.Remove("a");

            Assert.That(lifetimes["a"].IsAlive, Is.False);
            Assert.That(lifetimes["b"].IsAlive, Is.True);
        }

        [Test]
        public void ListSourceViewSeesItemsThatWereThereBefore()
        {
            var list = new BwViewableList<string> { "a" };
            var lifetimes = new Dictionary<string, Lifetime>();

            Readonly(list).View(_lifetime.Lifetime, (itemLifetime, item) => lifetimes.Add(item, itemLifetime));
            list.Remove("a");

            Assert.That(lifetimes.Keys, Is.EqualTo(new[] { "a" }));
            Assert.That(lifetimes["a"].IsAlive, Is.False);
        }

        [Test]
        public void ListSourceViewReplacesTheLifetimeOfAnUpdatedItem()
        {
            var list = new BwViewableList<string> { "a" };
            var lifetimes = new Dictionary<string, Lifetime>();
            Readonly(list).View(_lifetime.Lifetime, (itemLifetime, item) => lifetimes.Add(item, itemLifetime));

            list[0] = "b";

            Assert.That(lifetimes["a"].IsAlive, Is.False);
            Assert.That(lifetimes["b"].IsAlive, Is.True);
        }

        [Test]
        public void ListSourceViewEndsEveryItemLifetimeWithItsOwn()
        {
            var list = new BwViewableList<string> { "a", "b" };
            var lifetime = _lifetime.Lifetime.CreateNested();
            var lifetimes = new List<Lifetime>();
            Readonly(list).View(lifetime.Lifetime, (itemLifetime, _) => lifetimes.Add(itemLifetime));

            lifetime.Terminate();

            Assert.That(lifetimes, Has.Count.EqualTo(2));
            Assert.That(lifetimes.TrueForAll(itemLifetime => itemLifetime.IsNotAlive), Is.True);
        }

        [Test]
        public void ListSourceViewIgnoresChangesAfterItsLifetime()
        {
            var list = new BwViewableList<string>();
            var lifetime = _lifetime.Lifetime.CreateNested();
            Readonly(list).View(lifetime.Lifetime, (_, item) => _seen.Add(item));
            lifetime.Terminate();

            list.Add("a");

            Assert.That(_seen, Is.Empty);
        }

        [Test]
        public void ListViewGivesEveryItemALifetimeUntilItIsRemoved()
        {
            var list = new BwViewableList<string> { "a" };
            var lifetimes = new Dictionary<string, Lifetime>();
            list.View(_lifetime.Lifetime, (itemLifetime, item) => lifetimes.Add(item, itemLifetime));
            list.Add("b");

            list.RemoveAt(0);

            Assert.That(lifetimes["a"].IsAlive, Is.False);
            Assert.That(lifetimes["b"].IsAlive, Is.True);
        }

        [Test]
        public void ListViewEndsEveryItemLifetimeWithItsOwn()
        {
            var list = new BwViewableList<string> { "a", "b" };
            var lifetime = _lifetime.Lifetime.CreateNested();
            var lifetimes = new List<Lifetime>();
            list.View(lifetime.Lifetime, (itemLifetime, _) => lifetimes.Add(itemLifetime));

            lifetime.Terminate();

            Assert.That(lifetimes, Has.Count.EqualTo(2));
            Assert.That(lifetimes.TrueForAll(itemLifetime => itemLifetime.IsNotAlive), Is.True);
        }

        [Test]
        public void MapViewGivesEveryEntryALifetimeUntilItIsRemoved()
        {
            var map = new BwViewableMap<string, int> { ["a"] = 1 };
            var lifetimes = new Dictionary<string, Lifetime>();
            Readonly(map).View(_lifetime.Lifetime, (entryLifetime, key, value) => lifetimes.Add($"{key}{value}", entryLifetime));
            map.Add("b", 2);

            map.Remove("a");

            Assert.That(lifetimes.Keys, Is.EqualTo(new[] { "a1", "b2" }));
            Assert.That(lifetimes["a1"].IsAlive, Is.False);
            Assert.That(lifetimes["b2"].IsAlive, Is.True);
        }

        [Test]
        public void MapViewReplacesTheLifetimeOfAnUpdatedEntry()
        {
            var map = new BwViewableMap<string, int> { ["a"] = 1 };
            var lifetimes = new Dictionary<string, Lifetime>();
            Readonly(map).View(_lifetime.Lifetime, (entryLifetime, key, value) => lifetimes.Add($"{key}{value}", entryLifetime));

            map["a"] = 2;

            Assert.That(lifetimes["a1"].IsAlive, Is.False);
            Assert.That(lifetimes["a2"].IsAlive, Is.True);
        }

        [Test]
        public void MapViewEndsEveryEntryLifetimeWithItsOwn()
        {
            var map = new BwViewableMap<string, int> { ["a"] = 1, ["b"] = 2 };
            var lifetime = _lifetime.Lifetime.CreateNested();
            var lifetimes = new List<Lifetime>();
            Readonly(map).View(lifetime.Lifetime, (entryLifetime, _, _) => lifetimes.Add(entryLifetime));

            lifetime.Terminate();

            Assert.That(lifetimes, Has.Count.EqualTo(2));
            Assert.That(lifetimes.TrueForAll(entryLifetime => entryLifetime.IsNotAlive), Is.True);
        }

        [Test]
        public void MapViewIgnoresChangesAfterItsLifetime()
        {
            var map = new BwViewableMap<string, int>();
            var lifetime = _lifetime.Lifetime.CreateNested();
            Readonly(map).View(lifetime.Lifetime, (_, key, _) => _seen.Add(key));
            lifetime.Terminate();

            map.Add("a", 1);

            Assert.That(_seen, Is.Empty);
        }

        [Test]
        public void AdviseAddSeesAddedValuesIncludingUpdates()
        {
            var map = new ViewableMap<string, int> { ["a"] = 1 };
            map.AdviseAdd(_lifetime.Lifetime, (key, value) => _seen.Add($"{key}{value}"));

            map.Add("b", 2);
            map["a"] = 3;
            map.Remove("b");

            Assert.That(_seen, Is.EqualTo(new[] { "a1", "b2", "a3" }));
        }

        [Test]
        public void AdviseAddStopsWithItsLifetime()
        {
            var map = new ViewableMap<string, int>();
            var lifetime = _lifetime.Lifetime.CreateNested();
            map.AdviseAdd(lifetime.Lifetime, (key, value) => _seen.Add($"{key}{value}"));
            lifetime.Terminate();

            map.Add("a", 1);

            Assert.That(_seen, Is.Empty);
        }

        [Test]
        public void LifetimedEntryStaysUntilItsLifetimeEnds()
        {
            var map = new ViewableMap<string, int>();
            var entry = _lifetime.Lifetime.CreateNested();
            map.AddLifetimed(entry.Lifetime, "a", 1);
            var whileAlive = map.TryGetValue("a", out var value);

            entry.Terminate();

            Assert.That(whileAlive, Is.True);
            Assert.That(value, Is.EqualTo(1));
            Assert.That(map.ContainsKey("a"), Is.False);
        }

        [Test]
        public void LifetimedEntryRemovesOnlyItsOwnValue()
        {
            var map = new ViewableMap<string, int>();
            var entry = _lifetime.Lifetime.CreateNested();
            map.AddLifetimed(entry.Lifetime, "a", 1);
            map["a"] = 2;

            entry.Terminate();

            Assert.That(map["a"], Is.EqualTo(2));
        }

        [Test]
        public void LifetimedEntryOnAnEndedLifetimeIsRefused()
        {
            var map = new ViewableMap<string, int>();

            Assert.Throws<LifetimeCanceledException>(() => map.AddLifetimed(Lifetime.Terminated, "a", 1));
            Assert.That(map, Is.Empty);
        }

        [Test]
        public void ViewForKeyWatchesOnlyItsKey()
        {
            var map = new ViewableMap<string, int>();
            map.ViewForKey(_lifetime.Lifetime, "a", (_, value) => _seen.Add(value.ToString()));

            map.Add("b", 2);
            map.Add("a", 1);

            Assert.That(_seen, Is.EqualTo(new[] { "1" }));
        }

        [Test]
        public void ViewForKeySeesAnEntryThatWasThereBefore()
        {
            var map = new ViewableMap<string, int> { ["a"] = 1 };

            map.ViewForKey(_lifetime.Lifetime, "a", (_, value) => _seen.Add(value.ToString()));

            Assert.That(_seen, Is.EqualTo(new[] { "1" }));
        }

        [Test]
        public void ViewForKeyLifetimeEndsWhenTheKeyIsRemoved()
        {
            var map = new ViewableMap<string, int>();
            var lifetimes = new List<Lifetime>();
            map.ViewForKey(_lifetime.Lifetime, "a", (entryLifetime, _) => lifetimes.Add(entryLifetime));
            map.Add("a", 1);

            map.Remove("a");

            Assert.That(lifetimes, Has.Count.EqualTo(1));
            Assert.That(lifetimes[0].IsAlive, Is.False);
        }

        [Test]
        public void ViewForKeyGivesAFreshLifetimeForEveryNewValue()
        {
            var map = new ViewableMap<string, int>();
            var lifetimes = new List<Lifetime>();
            map.ViewForKey(_lifetime.Lifetime, "a", (entryLifetime, value) =>
            {
                lifetimes.Add(entryLifetime);
                _seen.Add(value.ToString());
            });
            map.Add("a", 1);

            map["a"] = 2;
            map.Remove("a");
            map.Add("a", 3);

            Assert.That(_seen, Is.EqualTo(new[] { "1", "2", "3" }));
            Assert.That(lifetimes[0].IsAlive, Is.False);
            Assert.That(lifetimes[1].IsAlive, Is.False);
            Assert.That(lifetimes[2].IsAlive, Is.True);
        }

        [Test]
        public void ViewForKeyLifetimeEndsWithItsOwn()
        {
            var map = new ViewableMap<string, int> { ["a"] = 1 };
            var lifetime = _lifetime.Lifetime.CreateNested();
            var lifetimes = new List<Lifetime>();
            map.ViewForKey(lifetime.Lifetime, "a", (entryLifetime, _) => lifetimes.Add(entryLifetime));

            lifetime.Terminate();
            map.Remove("a");
            map.Add("a", 2);

            Assert.That(lifetimes, Has.Count.EqualTo(1));
            Assert.That(lifetimes[0].IsAlive, Is.False);
        }

        private static IReadonlyViewableList<string> Readonly(BwViewableList<string> list) =>
            list;

        private static IReadonlyViewableMap<string, int> Readonly(BwViewableMap<string, int> map) =>
            map;
    }
}
