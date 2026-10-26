using Bw.Entities.Physics.Extensions;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Combat.Hands.Doubles;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Characters;
using Bw.UseCases.Combat.Hands.Network;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Combat.Weapon.Pickup;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Zenject;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Combat.Hands.Network
{
    public sealed class WeaponPickupServerHandlerTests
    {
        private const int WeaponLayer = 16;

        private const ulong GunId = 7;
        private const ulong FarGunId = 8;

        private CombatPhysics _physics;
        private WeaponPickupConfig _config;
        private WeaponCarriers _carriers;
        private FakeMortal _mortal;
        private FakeHands _hands;
        private FakeWeapon _gun;
        private FakeWeapon _farGun;
        private ManualResults<WeaponPickup> _requests;

        [SetUp]
        public void SetUp()
        {
            _physics = new CombatPhysics();
            _config = ScriptableObject.CreateInstance<WeaponPickupConfig>();
            _config.WeaponMask = 1 << WeaponLayer;
            _carriers = new WeaponCarriers();
            _mortal = new FakeMortal();
            _hands = new FakeHands();
            _gun = new FakeWeapon();
            _farGun = new FakeWeapon();
            _requests = new ManualResults<WeaponPickup>();
            var weapons = new WeaponCollections(_physics.Lifetime);
            weapons.ByIds.Add(GunId, _gun);
            weapons.ByIds.Add(FarGunId, _farGun);
            Lie(_gun, new Vector2(1.5f, 0f));
            Lie(_farGun, new Vector2(12f, 0f));
            var character = _physics.Space.CreateBody(_physics.Lifetime, _physics.Box("Character", Vector2.zero, new Vector2(1f, 2f), CombatPhysics.Character), PhysicsBody.BodyType.Kinematic);
            var pickables = new PickableWeapons(_carriers, _config, new WeaponPickupNetworkConfig(), character);
            new WeaponPickupServerHandler(_physics.Lifetime, _mortal, _requests, weapons, pickables, _hands);
        }

        [TearDown]
        public void TearDown()
        {
            _physics.Dispose();
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void RequestForAFreeWeaponWithinReachTakesIt()
        {
            _requests.Deliver(new WeaponPickup(GunId));

            Assert.That(_hands.Taken, Is.EqualTo(new[] { _gun }));
        }

        [Test]
        public void HandsNotFreeForTheWeaponTakeNothing()
        {
            _hands.Free = false;

            _requests.Deliver(new WeaponPickup(GunId));

            Assert.That(_hands.Taken, Is.Empty);
        }

        [Test]
        public void RequestForAWeaponOutOfReachTakesNothing()
        {
            _requests.Deliver(new WeaponPickup(FarGunId));

            Assert.That(_hands.Taken, Is.Empty);
        }

        [Test]
        public void RequestForAnUnknownWeaponTakesNothing()
        {
            _requests.Deliver(new WeaponPickup(99));

            Assert.That(_hands.Taken, Is.Empty);
        }

        [Test]
        public void CarriedWeaponIsNeverTaken()
        {
            _carriers.Add(Lifetime.Eternal, _gun, new Bw.UseCases.Combat.Characters.Character(new FakeMortal(), null, null, null));

            _requests.Deliver(new WeaponPickup(GunId));

            Assert.That(_hands.Taken, Is.Empty);
        }

        [Test]
        public void DeadCharacterPicksNothingUp()
        {
            _mortal.Die();

            _requests.Deliver(new WeaponPickup(GunId));

            Assert.That(_hands.Taken, Is.Empty);
        }

        [Test]
        public void RevivedCharacterPicksUpAgainOncePerRequest()
        {
            _mortal.Die();
            _mortal.Revive();

            _requests.Deliver(new WeaponPickup(GunId));

            Assert.That(_hands.Taken, Is.EqualTo(new[] { _gun }));
        }

        private void Lie(IWeapon weapon, Vector2 at)
        {
            var box = _physics.Box("Gun", at, new Vector2(1f, 0.3f), WeaponLayer);
            var container = new DiContainer();
            container.Bind<IWeapon>().FromInstance(weapon);
            container.Inject(box.gameObject.AddComponent<WeaponHolder>());
            _physics.Space.CreateBody(_physics.Lifetime, box, PhysicsBody.BodyType.Kinematic);
        }
    }
}
