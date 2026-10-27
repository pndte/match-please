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
        private const int Joined = 100;

        private LifetimeDefinition _lifetime;
        private ManualClock _clock;
        private CharacterInventory _inventory;
        private ManualResults<TickedState<CarriedWeapons>> _received;
        private ServerStates<int> _otherServer;
        private FakeWeapon _fists;
        private int _fistsSlot;
        private int _steps;

        [SetUp]
        public void SetUp()
        {
            var config = new WorldRollbackConfig { MaxReplayTicks = 4 };
            var inventoryConfig = AssetDatabase.LoadAssetAtPath<WeaponInventoryConfig>(InventoryConfigPath);
            _lifetime = new LifetimeDefinition();
            var lifetime = _lifetime.Lifetime;
            var kinds = new WeaponKindRegistry();
            var carriers = new WeaponCarriers();
            var weapons = new WeaponCollections(lifetime);
            var mortal = new FakeMortal();
            _inventory = new CharacterInventory(lifetime, mortal, carriers, kinds, inventoryConfig);
            var character = new FakeCharacter(mortal, _inventory);
            new CarriersRegistrar(lifetime, character, carriers);
            _received = new ManualResults<TickedState<CarriedWeapons>>();
            var receiver = new CarriedWeaponsReceiver(lifetime, character, _received, weapons, config);
            _otherServer = new ServerStates<int>(config.HistoryTicks);
            _clock = new ManualClock();
            var stepper = new RollbackStepper(lifetime, _clock, config);
            stepper.Stepped(TickPhase.Default).Advise(lifetime, _ => _steps++);
            stepper.Track(lifetime, new RollbackPart<InventoryContent>(_inventory, receiver, new ExactDivergence<InventoryContent>(), config));
            stepper.Track(lifetime, new RollbackPart<int>(new RestorableValue<int>(), _otherServer, new ExactDivergence<int>(), config));

            _fists = new FakeWeapon();
            kinds.AddLifetimed(lifetime, _fists, WeaponKind.Fists);
            weapons.ByIds.AddLifetimed(lifetime, FistsId, _fists);
            _fistsSlot = IndexOfSlotFor(inventoryConfig, WeaponKind.Fists);
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
