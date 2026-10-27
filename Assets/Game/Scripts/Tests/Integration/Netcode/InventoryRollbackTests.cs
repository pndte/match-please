using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Characters;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEditor;

namespace Bw.Tests.Integration.Netcode
{
    public sealed class InventoryRollbackTests
    {
        private const string InventoryConfigPath = "Assets/Game/Content/Weapons/WeaponInventoryConfig.asset";
        private const ulong FistsId = 7;
        private const ulong CrateId = 8;
        private const int Joined = 100;

        private LifetimeDefinition _lifetime;
        private ManualClock _clock;
        private WeaponKindRegistry _kinds;
        private WeaponCollections _weapons;
        private CharacterInventory _inventory;
        private ManualResults<TickedState<CarriedWeapons>> _received;
        private ServerStates<int> _otherServer;
        private FakeWeapon _fists;
        private int _fistsSlot;
        private int _propSlot;
        private int _steps;

        [SetUp]
        public void SetUp()
        {
            var config = new WorldRollbackConfig { MaxReplayTicks = 4 };
            var inventoryConfig = AssetDatabase.LoadAssetAtPath<WeaponInventoryConfig>(InventoryConfigPath);
            _lifetime = new LifetimeDefinition();
            var lifetime = _lifetime.Lifetime;
            _kinds = new WeaponKindRegistry();
            var carriers = new WeaponCarriers();
            _weapons = new WeaponCollections(lifetime);
            var mortal = new FakeMortal();
            _inventory = new CharacterInventory(lifetime, mortal, carriers, _kinds, inventoryConfig);
            var character = new FakeCharacter(mortal, _inventory);
            new CarriersRegistrar(lifetime, character, carriers);
            _received = new ManualResults<TickedState<CarriedWeapons>>();
            var receiver = new CarriedWeaponsReceiver(lifetime, character, _received, _weapons, config);
            _otherServer = new ServerStates<int>(config.HistoryTicks);
            _clock = new ManualClock();
            var stepper = new RollbackStepper(lifetime, _clock, config);
            stepper.Stepped(TickPhase.Default).Advise(lifetime, _ => _steps++);
            stepper.Track(lifetime, new RollbackPart<InventoryContent>(_inventory, receiver, new ExactDivergence<InventoryContent>(), config));
            stepper.Track(lifetime, new RollbackPart<int>(new RestorableValue<int>(), _otherServer, new ExactDivergence<int>(), config));

            _fists = new FakeWeapon();
            _kinds.AddLifetimed(lifetime, _fists, WeaponKind.Fists);
            _weapons.ByIds.AddLifetimed(lifetime, FistsId, _fists);
            _fistsSlot = IndexOfSlotFor(inventoryConfig, WeaponKind.Fists);
            _propSlot = IndexOfSlotFor(inventoryConfig, WeaponKind.Prop);
            _steps = 0;
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void FirstContentIsAppliedAtItsTick()
        {
            TickJoined();

            Assert.That(_inventory.Carried.ContainsKey(_fists), Is.True);
        }

        [Test]
        public void ReplayFromBeforeTheFirstContentKeepsTheCarriedFists()
        {
            TickJoined();
            _otherServer.Record(Joined + 2, 1);
            var steps = _steps;

            _clock.Tick(Joined + 6);

            Assert.That(_inventory.Carried.ContainsKey(_fists), Is.True, "a client that joined knew nothing of the inventory before its first content, and an empty guess is no state to roll back to");
            Assert.That(_steps - steps, Is.EqualTo(4), "the replay from the other part's divergence steps 103..105, then the tick itself");
        }

        [Test]
        public void ReplayFromBeforeACrateWasDespawnedLeavesTheCrateOut()
        {
            TickJoined();
            var crate = new FakeWeapon();
            var spawned = _lifetime.Lifetime.CreateNested();
            _kinds.AddLifetimed(spawned.Lifetime, crate, WeaponKind.Prop);
            _weapons.ByIds.AddLifetimed(spawned.Lifetime, CrateId, crate);
            _received.Deliver(new TickedState<CarriedWeapons>(Joined + 6, new CarriedWeapons(new[] { new CarriedWeapon(FistsId, (byte)_fistsSlot), new CarriedWeapon(CrateId, (byte)_propSlot) })));
            _clock.Tick(Joined + 6);
            _clock.Tick(Joined + 7);
            Assert.That(_inventory.Carried.ContainsKey(crate), Is.True, "the crate is in hands until it breaks");

            _received.Deliver(new TickedState<CarriedWeapons>(Joined + 8, new CarriedWeapons(new[] { new CarriedWeapon(FistsId, (byte)_fistsSlot) })));
            spawned.Terminate();
            _clock.Tick(Joined + 8);
            _clock.Tick(Joined + 9);
            _otherServer.Record(Joined + 7, 1);
            _clock.Tick(Joined + 10);

            Assert.That(_inventory.Carried.ContainsKey(crate), Is.False, "the broken crate was despawned, and a replay from a tick that had it in hands cannot bring it back");
            Assert.That(_inventory.Carried.ContainsKey(_fists), Is.True);
        }

        private void TickJoined()
        {
            for (var tick = Joined; tick < Joined + 5; tick++)
                _clock.Tick(tick);

            _received.Deliver(new TickedState<CarriedWeapons>(Joined + 3, new CarriedWeapons(new[] { new CarriedWeapon(FistsId, (byte)_fistsSlot) })));
            _clock.Tick(Joined + 5);
        }

        private static int IndexOfSlotFor(WeaponInventoryConfig config, WeaponKind kind)
        {
            for (var index = 0; index < config.Slots.Length; index++)
                if (Array.IndexOf(config.Slots[index].Kinds, kind) >= 0)
                    return index;

            throw new InvalidOperationException($"The inventory has no slot for {kind.ToString()}.");
        }
    }
}
