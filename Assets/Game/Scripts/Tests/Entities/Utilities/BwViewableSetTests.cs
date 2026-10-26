using System;
using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Extensions;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities
{
    public sealed class BwViewableSetTests
    {
        private LifetimeDefinition _lifetime;
        private BwViewableSet<string> _set;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _set = new BwViewableSet<string>();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void AddedItemIsContained()
        {
            _set.Add("Саня");

            Assert.That(_set.Contains("Саня"), Is.True);
            Assert.That(_set.Count, Is.EqualTo(1));
        }

        [Test]
        public void SecondAddOfTheSameItemThrows()
        {
            _set.Add("Саня");

            Assert.Throws<InvalidOperationException>(() => _set.Add("Саня"));
        }

        [Test]
        public void ViewReplaysTheItemsThenFollowsAdds()
        {
            var seen = new List<string>();
            _set.Add("Саня");

            _set.View(_lifetime.Lifetime, (_, item) => seen.Add(item));
            _set.Add("Сева");

            Assert.That(seen, Is.EqualTo(new[] { "Саня", "Сева" }));
        }

        [Test]
        public void ItemLifetimeEndsWhenTheItemIsRemoved()
        {
            var itemLifetime = Lifetime.Terminated;
            _set.Add("Саня");
            _set.View(_lifetime.Lifetime, (lifetime, _) => itemLifetime = lifetime);

            _set.Remove("Саня");

            Assert.That(itemLifetime.IsAlive, Is.False);
        }
    }
}
