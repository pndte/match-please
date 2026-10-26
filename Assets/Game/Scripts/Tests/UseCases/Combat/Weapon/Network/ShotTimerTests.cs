using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class ShotTimerTests
    {
        private LifetimeDefinition _lifetime;
        private FakeWeapon _weapon;
        private FakeFeed _feed;
        private FakeCharacter _carrier;
        private CharacterTrigger _trigger;
        private List<TimedShot> _timed;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _weapon = new FakeWeapon();
            _feed = new FakeFeed();
            _carrier = new FakeCharacter();
            _trigger = new CharacterTrigger();
            var carriers = new WeaponCarriers();
            carriers.Add(_lifetime.Lifetime, _weapon, _carrier);
            var triggers = new CharacterTriggers();
            triggers.ByCharacter.AddLifetimed(_lifetime.Lifetime, _carrier, _trigger);
            var timer = new ShotTimer(_lifetime.Lifetime, _weapon, _feed, carriers, triggers);
            _timed = new List<TimedShot>();
            timer.Timed.Advise(_lifetime.Lifetime, _timed.Add);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void FeedingAloneTimesNothing()
        {
            Tick(Held(6f, 2f));

            Assert.That(_timed, Is.Empty);
        }

        [Test]
        public void ShotIsTimedWithTheViewDelaysOfThePulledTrigger()
        {
            Tick(Held(6.5f, 2.25f));

            _weapon.Fire(30f);

            Assert.That(_timed, Has.Count.EqualTo(1));
            Assert.That(_timed[0].Timing.ViewDelay, Is.EqualTo(6.5f));
            Assert.That(_timed[0].Timing.PredictedDelay, Is.EqualTo(2.25f));
            Assert.That(_timed[0].Timing.LateTicks, Is.EqualTo(0));
        }

        [Test]
        public void ShotCarriesItsAimAndItsCarrierAsTheShooter()
        {
            Tick(Held(1f, 1f));

            _weapon.Fire(-45f);

            Assert.That(_timed[0].Shot.Aim, Is.EqualTo(-45f));
            Assert.That(_timed[0].Shooter, Is.SameAs(_carrier));
        }

        [Test]
        public void ShotAfterTheTriggerIsReleasedKeepsTheDelaysOfTheLastPull()
        {
            Tick(Held(6.5f, 2.25f));
            Tick(new TriggerControls(false, 9f, 9f, Press(1, 5, 0)));

            _weapon.Fire(0f);

            Assert.That(_timed[0].Timing.ViewDelay, Is.EqualTo(6.5f));
            Assert.That(_timed[0].Timing.PredictedDelay, Is.EqualTo(2.25f));
        }

        [Test]
        public void ShotOfALatePressIsTimedBackByItsLateness()
        {
            Tick(new TriggerControls(true, 4f, 1f, Press(1, 0, 3)));

            _weapon.Fire(0f);

            Assert.That(_timed[0].Timing.LateTicks, Is.EqualTo(3));
        }

        [Test]
        public void LatenessCountsOnlyForAFreshPress()
        {
            Tick(new TriggerControls(true, 4f, 1f, Press(1, 2, 3)));

            _weapon.Fire(0f);

            Assert.That(_timed[0].Timing.LateTicks, Is.EqualTo(0));
        }

        [Test]
        public void LateShotIsNeverTimedBeforeThePreviousShot()
        {
            Tick(new TriggerControls(true, 4f, 1f, Press(1, 0, 5)));
            _weapon.Fire(0f);

            Tick(new TriggerControls(true, 4f, 1f, Press(2, 0, 10)));
            _weapon.Fire(0f);

            Assert.That(_timed[0].Timing.LateTicks, Is.EqualTo(5));
            Assert.That(_timed[1].Timing.LateTicks, Is.EqualTo(5));
        }

        [Test]
        public void LateShotLongAfterThePreviousOneKeepsItsWholeLateness()
        {
            Tick(Held(4f, 1f));
            _weapon.Fire(0f);
            for (var tick = 0; tick < 10; tick++)
                Tick(Released());

            Tick(new TriggerControls(true, 4f, 1f, Press(2, 0, 4)));
            _weapon.Fire(0f);

            Assert.That(_timed[1].Timing.LateTicks, Is.EqualTo(4));
        }

        [Test]
        public void EveryShotIsTimedOnce()
        {
            Tick(Held(4f, 1f));
            _weapon.Fire(0f);
            Tick(Held(4f, 1f));
            _weapon.Fire(0f);

            Assert.That(_timed, Has.Count.EqualTo(2));
        }

        private void Tick(TriggerControls trigger)
        {
            _trigger.Take(trigger);
            _feed.Feed();
        }

        private static TriggerControls Held(float viewDelay, float predictedDelay) =>
            new(true, viewDelay, predictedDelay, Press(1, 1, 0));

        private static TriggerControls Released() =>
            new(false, 0f, 0f, Press(1, 9, 0));

        private static TriggerPress Press(byte number, byte age, byte late) =>
            new(number, age, 0f, 0f, late);
    }
}
