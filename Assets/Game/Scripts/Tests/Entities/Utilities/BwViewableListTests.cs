using System;
using System.Collections.Generic;
using Bw.Entities;
using JetBrains.Collections.Viewable;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities
{
    public sealed class BwViewableListTests
    {
        private LifetimeDefinition _lifetime;
        private BwViewableList<string> _list;
        private List<string> _events;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _list = new BwViewableList<string>();
            _events = new List<string>();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void AddAppendsAndAnnouncesItsIndex()
        {
            _list.Add("a");
            Follow();

            _list.Add("b");

            Assert.That(_list, Is.EqualTo(new[] { "a", "b" }));
            Assert.That(_events, Is.EqualTo(new[] { "Add 1 b" }));
        }

        [Test]
        public void InsertPutsTheItemAtItsIndex()
        {
            _list.Add("a");
            _list.Add("c");
            Follow();

            _list.Insert(1, "b");

            Assert.That(_list, Is.EqualTo(new[] { "a", "b", "c" }));
            Assert.That(_events, Is.EqualTo(new[] { "Add 1 b" }));
        }

        [Test]
        public void NullIsNeverAdded()
        {
            Follow();

            Assert.Throws<ArgumentNullException>(() => _list.Add(null));
            Assert.Throws<ArgumentNullException>(() => _list.Insert(0, null));
            Assert.That(_list, Is.Empty);
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void RemoveAtAnnouncesTheRemovedItem()
        {
            _list.Add("a");
            _list.Add("b");
            Follow();

            _list.RemoveAt(0);

            Assert.That(_list, Is.EqualTo(new[] { "b" }));
            Assert.That(_events, Is.EqualTo(new[] { "Remove 0 a" }));
        }

        [Test]
        public void RemoveFindsTheItem()
        {
            _list.Add("a");
            _list.Add("b");
            Follow();

            var removed = _list.Remove("b");

            Assert.That(removed, Is.True);
            Assert.That(_events, Is.EqualTo(new[] { "Remove 1 b" }));
        }

        [Test]
        public void RemovingAMissingItemChangesNothing()
        {
            _list.Add("a");
            Follow();

            var removed = _list.Remove("x");

            Assert.That(removed, Is.False);
            Assert.That(_list, Is.EqualTo(new[] { "a" }));
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void RemoveTakesTheFirstOfEqualItems()
        {
            _list.Add("a");
            _list.Add("b");
            _list.Add("a");
            Follow();

            _list.Remove("a");

            Assert.That(_list, Is.EqualTo(new[] { "b", "a" }));
            Assert.That(_events, Is.EqualTo(new[] { "Remove 0 a" }));
        }

        [Test]
        public void ClearRemovesFromTheEnd()
        {
            _list.Add("a");
            _list.Add("b");
            _list.Add("c");
            Follow();

            _list.Clear();

            Assert.That(_list, Is.Empty);
            Assert.That(_events, Is.EqualTo(new[] { "Remove 2 c", "Remove 1 b", "Remove 0 a" }));
        }

        [Test]
        public void IndexerAnnouncesAnUpdate()
        {
            _list.Add("a");
            Follow();

            _list[0] = "b";

            Assert.That(_list[0], Is.EqualTo("b"));
            Assert.That(_events, Is.EqualTo(new[] { "Update 0 a>b" }));
        }

        [Test]
        public void IndexerWithTheSameItemIsSilent()
        {
            _list.Add("a");
            Follow();

            _list[0] = "a";

            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void IndexerRefusesNull()
        {
            _list.Add("a");

            Assert.Throws<Assertion.AssertionException>(() => _list[0] = null);
            Assert.That(_list[0], Is.EqualTo("a"));
        }

        [Test]
        public void AdviseReplaysTheItemsThenFollowsChanges()
        {
            _list.Add("a");
            _list.Add("b");

            _list.Advise(_lifetime.Lifetime, change => _events.Add(Text(change)));
            _list.Add("c");

            Assert.That(_events, Is.EqualTo(new[] { "Add 0 a", "Add 1 b", "Add 2 c" }));
        }

        [Test]
        public void ChangeCarriesOnlyLaterEvents()
        {
            _list.Add("a");

            _list.Change.Advise(_lifetime.Lifetime, change => _events.Add(Text(change)));
            _list.Add("b");

            Assert.That(_events, Is.EqualTo(new[] { "Add 1 b" }));
        }

        [Test]
        public void AdviseStopsWithItsLifetime()
        {
            var lifetime = _lifetime.Lifetime.CreateNested();
            _list.Advise(lifetime.Lifetime, change => _events.Add(Text(change)));
            lifetime.Terminate();

            _list.Add("a");

            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void ListReadsLikeAList()
        {
            _list.Add("a");
            _list.Add("b");
            var copy = new string[3];

            _list.CopyTo(copy, 1);

            Assert.That(_list.Count, Is.EqualTo(2));
            Assert.That(_list.Contains("b"), Is.True);
            Assert.That(_list.Contains("x"), Is.False);
            Assert.That(_list.IndexOf("b"), Is.EqualTo(1));
            Assert.That(_list.IndexOf("x"), Is.EqualTo(-1));
            Assert.That(copy, Is.EqualTo(new[] { null, "a", "b" }));
            Assert.That(_list.IsReadOnly, Is.False);
        }

        [Test]
        public void GivenStorageIsTheListItself()
        {
            var storage = new List<string> { "a" };
            var list = new BwViewableList<string>(storage);

            list.Add("b");

            Assert.That(storage, Is.EqualTo(new[] { "a", "b" }));
            Assert.That(list, Is.EqualTo(new[] { "a", "b" }));
        }

        [Test]
        public void ReadonlyViewIsTheSameList()
        {
            IReadonlyViewableList<string> readonlyList = _list;
            _list.Add("a");

            Assert.That(readonlyList.Count, Is.EqualTo(1));
            Assert.That(readonlyList[0], Is.EqualTo("a"));
            Assert.That(readonlyList.Change, Is.SameAs(_list.Change));
        }

        private void Follow() =>
            _list.Change.Advise(_lifetime.Lifetime, change => _events.Add(Text(change)));

        private static string Text(ListEvent<string> change) =>
            change.Kind == AddUpdateRemove.Update
                ? $"Update {change.Index} {change.OldValue}>{change.NewValue}"
                : $"{change.Kind} {change.Index} {(change.Kind == AddUpdateRemove.Add ? change.NewValue : change.OldValue)}";
    }
}
