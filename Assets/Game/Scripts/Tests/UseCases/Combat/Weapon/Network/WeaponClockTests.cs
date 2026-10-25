using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Weapon.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class WeaponClockTests
    {
        private LifetimeDefinition _lifetime;
        private CountingSteps _rests;
        private FakeFeed _feed;
        private ScriptedWorldStep _world;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _rests = new CountingSteps();
            _feed = new FakeFeed();
            _world = new ScriptedWorldStep();
            new WeaponClock(_lifetime.Lifetime, _rests, _feed, _world);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void WeaponNobodyUsedRestsEveryTick()
        {
            _world.Step(1);
            _world.Step(2);
            _world.Step(3);

            Assert.That(_rests.Count, Is.EqualTo(3));
        }

        [Test]
        public void WeaponFedThisTickDoesNotRest()
        {
            _feed.Feed();

            _world.Step(1);

            Assert.That(_rests.Count, Is.EqualTo(0));
        }

        [Test]
        public void UseCountsForOneTickOnly()
        {
            _feed.Feed();
            _world.Step(1);

            _world.Step(2);

            Assert.That(_rests.Count, Is.EqualTo(1));
        }

        [Test]
        public void SeveralFeedsInATickStillMeanOneUsedTick()
        {
            _feed.Feed();
            _feed.Feed();
            _world.Step(1);

            _world.Step(2);

            Assert.That(_rests.Count, Is.EqualTo(1));
        }

        [Test]
        public void RestComesAtTheLatePhase()
        {
            _world.Fire(TickPhase.Prepare, 1);
            _world.Fire(TickPhase.Early, 1);
            _world.Fire(TickPhase.Default, 1);

            Assert.That(_rests.Count, Is.EqualTo(0));

            _world.Fire(TickPhase.Late, 1);

            Assert.That(_rests.Count, Is.EqualTo(1));
        }

        [Test]
        public void ReplayedIdleTicksRestTheWeaponAgain()
        {
            _world.Step(10);

            _world.Replay(8, 9);

            Assert.That(_rests.Count, Is.EqualTo(3));
        }

        [Test]
        public void ClockStopsWithItsLifetime()
        {
            _lifetime.Terminate();

            _world.Step(1);

            Assert.That(_rests.Count, Is.EqualTo(0));
        }
    }
}
