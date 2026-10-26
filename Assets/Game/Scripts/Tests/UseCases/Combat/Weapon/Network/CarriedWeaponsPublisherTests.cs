using System.Linq;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class CarriedWeaponsPublisherTests
    {
        private const int Heartbeat = 10;

        private LifetimeDefinition _lifetime;
        private WeaponSlot _gun;
        private RecordingInventory _inventory;
        private WeaponCollections _weapons;
        private RecordingResults<TickedState<CarriedWeapons>> _sent;
        private ScriptedWorldStep _world;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _gun = new WeaponSlot();
            _inventory = new RecordingInventory(_gun);
            _weapons = new WeaponCollections(_lifetime.Lifetime);
            _sent = new RecordingResults<TickedState<CarriedWeapons>>();
            _world = new ScriptedWorldStep();
            var config = new WorldRollbackConfig { UnchangedStateTicks = Heartbeat };
            new CarriedWeaponsPublisher(_lifetime.Lifetime, _inventory, new CarriedWeaponsReport(_inventory, _weapons), _sent, _world, config);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void FirstPublishSendsTheContent()
        {
            _world.Step(5);

            Assert.That(_sent.Broadcasts.Select(sent => sent.Tick), Is.EqualTo(new[] { 5 }));
            Assert.That(_sent.Broadcasts[0].State.Weapons, Is.Empty);
        }

        [Test]
        public void UnchangedContentWaitsForTheHeartbeat()
        {
            for (var tick = 1; tick <= 1 + Heartbeat * 2; tick++)
                _world.Step(tick);

            Assert.That(_sent.Broadcasts.Select(sent => sent.Tick), Is.EqualTo(new[] { 1, 1 + Heartbeat, 1 + Heartbeat * 2 }));
        }

        [Test]
        public void ChangeIsSentOnTheTicksPublish()
        {
            _world.Step(1);
            _world.Step(2);
            Give(4);

            _world.Step(3);

            Assert.That(_sent.Broadcasts.Select(sent => sent.Tick), Is.EqualTo(new[] { 1, 3 }));
            Assert.That(_sent.Broadcasts[1].State.Weapons, Is.EqualTo(new[] { new CarriedWeapon(4, 0) }));
        }

        [Test]
        public void ChangeRestartsTheHeartbeat()
        {
            _world.Step(1);
            Give(4);
            _world.Step(3);

            for (var tick = 4; tick <= 3 + Heartbeat; tick++)
                _world.Step(tick);

            Assert.That(_sent.Broadcasts.Select(sent => sent.Tick), Is.EqualTo(new[] { 1, 3, 3 + Heartbeat }));
        }

        [Test]
        public void SeveralChangesInATickAreSentOnce()
        {
            _world.Step(1);
            var weapon = Give(4);
            _inventory.Drop(weapon);
            Give(5);

            _world.Step(2);

            Assert.That(_sent.Broadcasts, Has.Count.EqualTo(2));
            Assert.That(_sent.Broadcasts[1].State.Weapons, Is.EqualTo(new[] { new CarriedWeapon(5, 0) }));
        }

        [Test]
        public void OnlyThePublishPhaseSends()
        {
            foreach (var phase in new[] { TickPhase.Prepare, TickPhase.Early, TickPhase.Default, TickPhase.Late })
                _world.Fire(phase, 1);

            Assert.That(_sent.Broadcasts, Is.Empty);
        }

        private FakeWeapon Give(ulong id)
        {
            var weapon = new FakeWeapon();
            _weapons.ByIds.AddLifetimed(_lifetime.Lifetime, id, weapon);
            _inventory.Give(_gun, weapon);
            return weapon;
        }
    }
}
