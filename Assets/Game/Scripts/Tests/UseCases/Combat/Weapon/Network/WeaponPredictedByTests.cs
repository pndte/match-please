using Bw.Entities.Extensions;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class WeaponPredictedByTests
    {
        private LifetimeDefinition _lifetime;
        private FakeWeapon _weapon;
        private WeaponCarriers _carriers;
        private CharacterPredictions _characters;
        private FakeBubble _floor;
        private FakeCharacter _holder;
        private ManualControl _holderPrediction;
        private WeaponPredictedBy _predictedBy;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _weapon = new FakeWeapon();
            _carriers = new WeaponCarriers();
            _characters = new CharacterPredictions();
            _floor = new FakeBubble();
            _holder = new FakeCharacter();
            _holderPrediction = new ManualControl();
            _predictedBy = new WeaponPredictedBy(_lifetime.Lifetime, _weapon, _carriers, _characters, _floor);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void WeaponOnTheFloorOutsideTheBubbleIsNotPredicted()
        {
            Assert.That(_predictedBy.Me.Value, Is.False);
        }

        [Test]
        public void WeaponOnTheFloorFollowsTheBubble()
        {
            _floor.Enter();

            Assert.That(_predictedBy.Me.Value, Is.True);

            _floor.Leave();

            Assert.That(_predictedBy.Me.Value, Is.False);
        }

        [Test]
        public void CarriedWeaponIsPredictedWithItsHolder()
        {
            _characters.ByCharacter.AddLifetimed(_lifetime.Lifetime, _holder, _holderPrediction);
            _holderPrediction.Take();

            Carry(_lifetime.Lifetime);

            Assert.That(_predictedBy.Me.Value, Is.True);
        }

        [Test]
        public void CarriedWeaponOfAHolderNotPredictedHereIsNotPredictedInsideTheBubble()
        {
            _floor.Enter();
            _characters.ByCharacter.AddLifetimed(_lifetime.Lifetime, _holder, _holderPrediction);

            Carry(_lifetime.Lifetime);

            Assert.That(_predictedBy.Me.Value, Is.False);
        }

        [Test]
        public void CarriedWeaponFollowsItsHoldersPredictionAsItChanges()
        {
            _characters.ByCharacter.AddLifetimed(_lifetime.Lifetime, _holder, _holderPrediction);
            Carry(_lifetime.Lifetime);

            _holderPrediction.Take();

            Assert.That(_predictedBy.Me.Value, Is.True);

            _holderPrediction.Release();

            Assert.That(_predictedBy.Me.Value, Is.False);
        }

        [Test]
        public void CarriedWeaponIgnoresTheBubble()
        {
            _characters.ByCharacter.AddLifetimed(_lifetime.Lifetime, _holder, _holderPrediction);
            _holderPrediction.Take();
            Carry(_lifetime.Lifetime);

            _floor.Enter();
            _floor.Leave();

            Assert.That(_predictedBy.Me.Value, Is.True);
        }

        [Test]
        public void HolderWithoutAPredictionLeavesTheWeaponUnpredicted()
        {
            _floor.Enter();

            Carry(_lifetime.Lifetime);

            Assert.That(_predictedBy.Me.Value, Is.False);
        }

        [Test]
        public void HolderRegisteredAfterThePickupIsFollowed()
        {
            Carry(_lifetime.Lifetime);
            _holderPrediction.Take();

            _characters.ByCharacter.AddLifetimed(_lifetime.Lifetime, _holder, _holderPrediction);

            Assert.That(_predictedBy.Me.Value, Is.True);
        }

        [Test]
        public void DroppedWeaponGoesBackToTheBubble()
        {
            _floor.Enter();
            _characters.ByCharacter.AddLifetimed(_lifetime.Lifetime, _holder, _holderPrediction);
            var carried = new LifetimeDefinition();
            Carry(carried.Lifetime);

            carried.Terminate();

            Assert.That(_predictedBy.Me.Value, Is.True);
        }

        [Test]
        public void DroppedWeaponStopsFollowingItsFormerHolder()
        {
            _characters.ByCharacter.AddLifetimed(_lifetime.Lifetime, _holder, _holderPrediction);
            var carried = new LifetimeDefinition();
            Carry(carried.Lifetime);
            carried.Terminate();

            _holderPrediction.Take();

            Assert.That(_predictedBy.Me.Value, Is.False);
        }

        private void Carry(Lifetime lifetime) =>
            _carriers.ByWeapon.AddLifetimed(lifetime, _weapon, _holder);
    }
}
