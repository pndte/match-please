using System;
using Bw.UseCases;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases
{
    public sealed class OwnershipTests
    {
        [Test]
        public void ObjectIsNotMineAtFirst()
        {
            Assert.That(new Ownership().Mine.Value, Is.False);
        }

        [Test]
        public void OwnershipIsMineForTheGivenLifetime()
        {
            var ownership = new Ownership();
            var owned = new LifetimeDefinition();

            ownership.SetMine(owned.Lifetime);

            Assert.That(ownership.Mine.Value, Is.True);

            owned.Terminate();

            Assert.That(ownership.Mine.Value, Is.False);
        }

        [Test]
        public void OwnershipIsGivenOnceAtATime()
        {
            var ownership = new Ownership();
            ownership.SetMine(new LifetimeDefinition().Lifetime);

            Assert.Throws<InvalidOperationException>(() => ownership.SetMine(new LifetimeDefinition().Lifetime));
        }

        [Test]
        public void OwnershipIsGivenAgainAfterThePreviousEnds()
        {
            var ownership = new Ownership();
            var first = new LifetimeDefinition();
            ownership.SetMine(first.Lifetime);
            first.Terminate();

            ownership.SetMine(new LifetimeDefinition().Lifetime);

            Assert.That(ownership.Mine.Value, Is.True);
        }

        [Test]
        public void OwnershipForAnEndedLifetimeIsRefusedWithoutTakingIt()
        {
            var ownership = new Ownership();

            Assert.Throws<InvalidOperationException>(() => ownership.SetMine(Lifetime.Terminated));
            Assert.That(ownership.Mine.Value, Is.False);
        }
    }
}
