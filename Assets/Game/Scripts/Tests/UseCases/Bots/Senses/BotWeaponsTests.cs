using Bw.Tests.Entities.Network.Doubles;
using Bw.Tests.UseCases.PropsMatchBots.Doubles;
using Bw.UseCases.Bots.Senses;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Melee;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Props;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Bots.Senses
{
    public sealed class BotWeaponsTests
    {
        private LifetimeDefinition _lifetime;
        private WeaponStates<GunState> _guns;
        private BotWeapons _weapons;
        private StubCharacter _target;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _guns = new WeaponStates<GunState>();
            _weapons = new BotWeapons(new StubWeaponCarriers(), _guns, new WeaponStates<MeleeState>(), new WeaponStates<PropState>());
            _target = new StubCharacter();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void GunInHandsWithReloadTicksLeftIsReloading()
        {
            Hold(WeaponKind.Gun, new GunState(0f, 0, 0, 30, 5));

            Assert.That(_weapons.IsReloading(_target), Is.True);
        }

        [Test]
        public void LoadedGunInHandsIsNotReloading()
        {
            Hold(WeaponKind.Gun, new GunState(0f, 12, 3, 0, 5));

            Assert.That(_weapons.IsReloading(_target), Is.False);
        }

        [Test]
        public void FistsInHandsAreNotReloading()
        {
            Hold(WeaponKind.Fists, default);

            Assert.That(_weapons.IsReloading(_target), Is.False);
        }

        [Test]
        public void EmptyHandsAreNotReloading()
        {
            Assert.That(_weapons.IsReloading(_target), Is.False);
        }

        private void Hold(WeaponKind kind, GunState gun)
        {
            var weapon = new StubWeapon();
            _target.Inventory.Carry(new WeaponSlot(), weapon, new WeaponKindRules { Kind = kind });
            _target.Hands.InHands = weapon;
            if (kind == WeaponKind.Gun)
                _guns.Add(_lifetime.Lifetime, weapon, new SimulationValue<GunState>(gun));
        }
    }
}
