using System;
using Bw.Entities.Players;
using Bw.UseCases;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases
{
    public sealed class OwnerRegistryTests
    {
        [Test]
        public void OwnerIsListedWhileItsLifetimeLasts()
        {
            var registry = new OwnerRegistry();
            var player = new Player("Саня");
            var owned = new LifetimeDefinition();

            registry.AddOwner(owned.Lifetime, player);
            var listed = registry.Owners.Contains(player);
            owned.Terminate();

            Assert.That(listed, Is.True);
            Assert.That(registry.Owners.Contains(player), Is.False);
        }

        [Test]
        public void SamePlayerOwnsOnceAtATime()
        {
            var registry = new OwnerRegistry();
            var player = new Player("Саня");
            registry.AddOwner(Lifetime.Eternal, player);

            Assert.Throws<InvalidOperationException>(() => registry.AddOwner(Lifetime.Eternal, player));
        }
    }
}
