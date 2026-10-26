using System;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.Network.Guarding;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Mortality.Guarding
{
    public sealed class GuardCoversTests
    {
        private static readonly Contact Right = new(new Vector2(1f, 0f));

        private LifetimeDefinition _lifetime;
        private GuardCovers _covers;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _covers = new GuardCovers();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void MortalWithoutAGuardIsCoveredNowhere() =>
            Assert.That(_covers.CoverOf(new FakeMortal()).InCover(Right), Is.False);

        [Test]
        public void RegisteredGuardCoversItsMortal()
        {
            var mortal = new FakeMortal();

            new GuardCoverRegistrar(_lifetime.Lifetime, mortal, new EveryCover(), _covers);

            Assert.That(_covers.CoverOf(mortal).InCover(Right), Is.True);
            Assert.That(_covers.CoverOf(new FakeMortal()).InCover(Right), Is.False);
        }

        [Test]
        public void GuardEndsWithItsLifetime()
        {
            var mortal = new FakeMortal();
            var guarded = new LifetimeDefinition();
            _covers.AddLifetimed(guarded.Lifetime, mortal, new EveryCover());

            guarded.Terminate();

            Assert.That(_covers.CoverOf(mortal).InCover(Right), Is.False);
        }

        [Test]
        public void SecondGuardOfOneMortalThrows()
        {
            var mortal = new FakeMortal();
            _covers.AddLifetimed(_lifetime.Lifetime, mortal, new EveryCover());

            Assert.Throws<InvalidOperationException>(() => _covers.AddLifetimed(_lifetime.Lifetime, mortal, new EveryCover()));
        }

        private sealed class EveryCover : IGuardCover
        {
            public bool InCover(Contact contact) =>
                true;
        }
    }
}
