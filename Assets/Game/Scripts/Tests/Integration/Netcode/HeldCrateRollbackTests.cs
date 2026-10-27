using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Characters;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Network.Prediction;
using Bw.UseCases.Props;
using Bw.UseCases.Props.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.Integration.Netcode
{
    public sealed class HeldCrateRollbackTests
    {
        private const string InventoryConfigPath = "Assets/Game/Content/Weapons/WeaponInventoryConfig.asset";
        private const string CrateConfigPath = "Assets/Game/Content/Crates/CrateBodyConfig.asset";
        private const string HeldConfigPath = "Assets/Game/Content/Props/HeldPropConfig.asset";
        private const string ThrowConfigPath = "Assets/Game/Content/Props/PropThrowConfig.asset";
        private const ulong CrateId = 7;
        private const int Start = 96;
        private const int PickedUp = 104;
        private const int Delay = 4;
        private const int End = 124;

        private static readonly Vector2 Hands = new(0f, 1.5f);
        private static readonly Vector2 Floor = new(3f, 0.5f);

        private PhysicsTestWorld _world;
        private PhysicsBody _body;
        private DynamicBody _crate;
        private FakeWeapon _prop;
        private CharacterInventory _inventory;
        private ManualResults<TickedState<CarriedWeapons>> _carried;
        private ManualResults<TickedState<RigidState>> _crateStates;
        private ServerStates<int> _cluster;
        private int _propSlot;
        private int _releases;

        [SetUp]
        public void SetUp()
        {
            var config = new WorldRollbackConfig { MaxReplayTicks = 8 };
            var bodyConfig = AssetDatabase.LoadAssetAtPath<DynamicBodyConfig>(CrateConfigPath);
            var inventoryConfig = AssetDatabase.LoadAssetAtPath<WeaponInventoryConfig>(InventoryConfigPath);
            _world = new PhysicsTestWorld();
            var lifetime = _world.Lifetime;
            _world.Static("Floor", new Vector2(0f, -0.5f), new Vector2(40f, 1f));
            var box = _world.Box("Crate", Floor, Vector2.one);
            _body = bodyConfig.CreateBody(_world.Space, lifetime, box, PhysicsBody.BodyType.Dynamic);
            _crate = new DynamicBody(lifetime, _body, _world.Space);

            var kinds = new WeaponKindRegistry();
            var carriers = new WeaponCarriers();
            var weapons = new WeaponCollections(lifetime);
            _prop = new FakeWeapon();
            kinds.AddLifetimed(lifetime, _prop, WeaponKind.Prop);
            weapons.ByIds.AddLifetimed(lifetime, CrateId, _prop);
            var mortal = new FakeMortal();
            _inventory = new CharacterInventory(lifetime, mortal, carriers, kinds, inventoryConfig);
            var holder = new FakeCharacter(mortal, _inventory);
            holder.Hands.MoveTo(Hands);
            holder.Hands.Grip = new GripPose(Hands, 0f, Vector2.zero);
            new CarriersRegistrar(lifetime, holder, carriers);
            _propSlot = IndexOfSlotFor(inventoryConfig, WeaponKind.Prop);

            var stepper = new RollbackStepper(lifetime, _world.Clock, config);
            new PhysicsStepper(lifetime, _world.Space, stepper);
            var networkObject = new FakeNetworkObject();
            var spots = new ReleaseSpots(_world.Space, _body, box, bodyConfig, AssetDatabase.LoadAssetAtPath<HeldPropConfig>(HeldConfigPath));
            var launcher = new PropLauncher(_crate, _body, spots, AssetDatabase.LoadAssetAtPath<PropThrowConfig>(ThrowConfigPath));
            new HeldWeaponBody(lifetime, networkObject, _prop, carriers, _body);
            var releaser = new PropReleaser(lifetime, networkObject, _prop, carriers, launcher, stepper);
            releaser.Released.Advise(lifetime, _ => _releases++);
            networkObject.Spawn();

            _crateStates = new ManualResults<TickedState<RigidState>>();
            stepper.Track(lifetime, new RollbackPart<RigidState>(_crate, new ReplicatedStates<RigidState>(lifetime, _crateStates, config), new RigidDivergence(), config));
            _carried = new ManualResults<TickedState<CarriedWeapons>>();
            var receiver = new CarriedWeaponsReceiver(lifetime, holder, _carried, weapons, config);
            stepper.Track(lifetime, new RollbackPart<InventoryContent>(_inventory, receiver, new ExactDivergence<InventoryContent>(), config));
            _cluster = new ServerStates<int>(config.HistoryTicks);
            stepper.Track(lifetime, new RollbackPart<int>(new RestorableValue<int>(), _cluster, new ExactDivergence<int>(), config));
            _releases = 0;
        }

        [TearDown]
        public void TearDown() =>
            _world.Dispose();

        [Test]
        public void CrateTheServerPutInTheHandsStaysThereThroughEveryLaterReplay()
        {
            for (var tick = Start; tick <= End; tick++)
            {
                if (tick == Start + 1)
                {
                    _carried.Deliver(new TickedState<CarriedWeapons>(Start, new CarriedWeapons(Array.Empty<CarriedWeapon>())));
                    _crateStates.Deliver(new TickedState<RigidState>(Start, _crate.State.Value));
                }

                if (tick == PickedUp + Delay)
                    _carried.Deliver(new TickedState<CarriedWeapons>(PickedUp, new CarriedWeapons(new[] { new CarriedWeapon(CrateId, (byte)_propSlot) })));

                if (tick > PickedUp + Delay)
                    _cluster.Record(tick - 6, tick);

                var releasesBefore = _releases;
                _world.Clock.Tick(tick);

                if (tick < PickedUp + Delay)
                    continue;

                Assert.That(_inventory.Carried.ContainsKey(_prop), Is.True, $"tick {tick}: a replay from before the pickup took the crate out of the hands, and the client drew it outside them");
                Assert.That(_body.enabled, Is.False, $"tick {tick}: the held crate's body is back in the world");
                Assert.That(_releases, Is.EqualTo(releasesBefore), $"tick {tick}: the client released a crate the server never released");
            }
        }

        [Test]
        public void CrateTheServerLetGoOfFollowsTheServersStateAndNotTheClientsOwnPlace()
        {
            var serverSpot = new Vector2(1.6f, 1.5f);
            for (var tick = Start; tick <= End; tick++)
            {
                if (tick == Start + 1)
                {
                    _carried.Deliver(new TickedState<CarriedWeapons>(Start, new CarriedWeapons(new[] { new CarriedWeapon(CrateId, (byte)_propSlot) })));
                    _crateStates.Deliver(new TickedState<RigidState>(Start, _crate.State.Value));
                }

                if (tick == PickedUp + Delay)
                {
                    _carried.Deliver(new TickedState<CarriedWeapons>(PickedUp, new CarriedWeapons(Array.Empty<CarriedWeapon>())));
                    _crateStates.Deliver(new TickedState<RigidState>(PickedUp, new RigidState(serverSpot, 0f, Vector2.zero, 0f)));
                }

                if (tick > PickedUp + Delay)
                    _cluster.Record(tick - 6, tick);

                var releasesBefore = _releases;
                _world.Clock.Tick(tick);

                if (tick < PickedUp + Delay)
                    continue;

                Assert.That(_releases, Is.EqualTo(releasesBefore), $"tick {tick}: the client placed a crate the server had already placed");
                Assert.That(_crate.State.Value.Position.x, Is.EqualTo(serverSpot.x).Within(0.05f), $"tick {tick}: the crate left the server's place for the client's own");
            }
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
