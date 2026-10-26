using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class CarriedWeaponsReceiverTests
    {
        private const int MaxReplayTicks = 4;

        private LifetimeDefinition _lifetime;
        private WeaponSlot _fists;
        private WeaponSlot _gun;
        private FakeCharacter _character;
        private ManualResults<TickedState<CarriedWeapons>> _received;
        private WeaponCollections _weapons;
        private CarriedWeaponsReceiver _receiver;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _fists = new WeaponSlot();
            _gun = new WeaponSlot();
            _character = new FakeCharacter(new FakeMortal(), new RecordingInventory(_fists, _gun));
            _received = new ManualResults<TickedState<CarriedWeapons>>();
            _weapons = new WeaponCollections(_lifetime.Lifetime);
            var config = new WorldRollbackConfig { MaxReplayTicks = MaxReplayTicks };
            _receiver = new CarriedWeaponsReceiver(_lifetime.Lifetime, _character, _received, _weapons, config);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void HistoryKeepsTheReplayWindowAndOneTickMore()
        {
            Assert.That(_receiver.Capacity, Is.EqualTo(MaxReplayTicks + 1));
        }

        [Test]
        public void ReceivedWeaponsAreRecordedAtTheirTickInTheirSlots()
        {
            var fists = Spawned(3);
            var gun = Spawned(8);

            _received.Deliver(Carried(20, new CarriedWeapon(3, 0), new CarriedWeapon(8, 1)));

            Assert.That(_receiver.TryGetAt(20, out var content), Is.True);
            Assert.That(content.Items.Count, Is.EqualTo(2));
            Assert.That(content.IsListed(fists, _fists), Is.True);
            Assert.That(content.IsListed(gun, _gun), Is.True);
        }

        [Test]
        public void EmptyHandsAreRecordedAsEmptyContent()
        {
            _received.Deliver(Carried(20));

            Assert.That(_receiver.TryGetAt(20, out var content), Is.True);
            Assert.That(content.Items, Is.Empty);
        }

        [Test]
        public void WeaponNotYetSpawnedHereHoldsTheRecordBack()
        {
            Spawned(3);

            _received.Deliver(Carried(20, new CarriedWeapon(3, 0), new CarriedWeapon(8, 1)));

            Assert.That(_receiver.TryGetAt(20, out _), Is.False);
        }

        [Test]
        public void HeldBackRecordIsMadeOnceItsWeaponsAreSpawned()
        {
            var fists = Spawned(3);
            _received.Deliver(Carried(20, new CarriedWeapon(3, 0), new CarriedWeapon(8, 1)));

            var gun = Spawned(8);

            Assert.That(_receiver.TryGetAt(20, out var content), Is.True);
            Assert.That(content.IsListed(fists, _fists), Is.True);
            Assert.That(content.IsListed(gun, _gun), Is.True);
        }

        [Test]
        public void NewerReportReplacesTheOneHeldBack()
        {
            Spawned(3);
            _received.Deliver(Carried(20, new CarriedWeapon(8, 1)));
            _received.Deliver(Carried(21, new CarriedWeapon(3, 0)));

            Spawned(8);

            Assert.That(_receiver.TryGetAt(20, out _), Is.False);
            Assert.That(_receiver.TryGetAt(21, out _), Is.True);
        }

        [Test]
        public void ReportOlderThanTheHistoryIsDropped()
        {
            _received.Deliver(Carried(100));

            _received.Deliver(Carried(100 - MaxReplayTicks - 1));

            Assert.That(_receiver.NewestTick, Is.EqualTo(100));
            Assert.That(_receiver.TryGetAt(100 - MaxReplayTicks - 1, out _), Is.False);
        }

        [Test]
        public void ReportAtTheEdgeOfTheHistoryIsRecorded()
        {
            _received.Deliver(Carried(100));

            _received.Deliver(Carried(100 - MaxReplayTicks));

            Assert.That(_receiver.TryGetAt(100 - MaxReplayTicks, out _), Is.True);
        }

        [Test]
        public void DeathForgetsTheHistory()
        {
            _received.Deliver(Carried(20));

            _character.Mortal.Die();

            Assert.That(_receiver.TryGetAt(20, out _), Is.False);
            Assert.That(_receiver.NewestTick, Is.EqualTo(int.MinValue));
        }

        [Test]
        public void ReportsReceivedWhileDeadAreIgnored()
        {
            _character.Mortal.Die();

            _received.Deliver(Carried(20));

            Assert.That(_receiver.TryGetAt(20, out _), Is.False);
        }

        [Test]
        public void ReportHeldBackAtDeathIsNeverRecorded()
        {
            _received.Deliver(Carried(20, new CarriedWeapon(8, 1)));
            _character.Mortal.Die();
            _character.Mortal.Live();

            Spawned(8);

            Assert.That(_receiver.TryGetAt(20, out _), Is.False);
        }

        [Test]
        public void NextLifeReceivesAgain()
        {
            _character.Mortal.Die();
            _character.Mortal.Live();

            _received.Deliver(Carried(30));

            Assert.That(_receiver.TryGetAt(30, out _), Is.True);
        }

        private FakeWeapon Spawned(ulong id)
        {
            var weapon = new FakeWeapon();
            _weapons.ByIds.AddLifetimed(_lifetime.Lifetime, id, weapon);
            return weapon;
        }

        private static TickedState<CarriedWeapons> Carried(int tick, params CarriedWeapon[] weapons) =>
            new(tick, new CarriedWeapons(weapons));
    }
}
