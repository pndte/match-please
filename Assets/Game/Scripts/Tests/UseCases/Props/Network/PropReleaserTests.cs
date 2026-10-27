using Bw.Tests.Entities.Physics.Doubles;
using Bw.Entities.Network.Ticks;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Props;
using Bw.UseCases.Props.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Props.Network
{
    public sealed class PropReleaserTests
    {
        private static readonly Vector2 Hands = new(3f, 1.5f);
        private static readonly Vector2 Size = Vector2.one;

        private PhysicsTestWorld _world;
        private PhysicsBody _body;
        private FakeNetworkObject _networkObject;
        private FakeWeapon _prop;
        private WeaponCarriers _carriers;
        private FakeCharacter _holder;
        private PropReleaser _releaser;
        private ScriptedWorldStep _worldStep;
        private LifetimeDefinition _carried;

        [SetUp]
        public void SetUp()
        {
            var bodyConfig = AssetDatabase.LoadAssetAtPath<DynamicBodyConfig>("Assets/Game/Content/Crates/CrateBodyConfig.asset");
            var heldConfig = AssetDatabase.LoadAssetAtPath<HeldPropConfig>("Assets/Game/Content/Props/HeldPropConfig.asset");
            var throwConfig = AssetDatabase.LoadAssetAtPath<PropThrowConfig>("Assets/Game/Content/Props/PropThrowConfig.asset");
            _world = new PhysicsTestWorld();
            var box = _world.Box("Crate", new Vector2(-5f, 0.5f), Size);
            _body = bodyConfig.CreateBody(_world.Space, _world.Lifetime, box, PhysicsBody.BodyType.Kinematic);
            var spots = new ReleaseSpots(_world.Space, _body, box, bodyConfig, heldConfig);
            var launcher = new PropLauncher(new DynamicBody(_world.Lifetime, _body, _world.Space), _body, spots, throwConfig);
            _networkObject = new FakeNetworkObject();
            _prop = new FakeWeapon();
            _carriers = new WeaponCarriers();
            _holder = new FakeCharacter();
            _holder.Hands.MoveTo(Hands);
            _holder.Hands.Grip = new GripPose(Hands, 0f, Vector2.zero);
            new HeldWeaponBody(_world.Lifetime, _networkObject, _prop, _carriers, _body);
            _worldStep = new ScriptedWorldStep();
            _releaser = new PropReleaser(_world.Lifetime, _networkObject, _prop, _carriers, launcher, _worldStep);
            _networkObject.Spawn();
            _carried = new LifetimeDefinition();
            _holder.Inventory.Give(new WeaponSlot(), _prop);
            _carriers.Add(_carried.Lifetime, _prop, _holder);
        }

        [TearDown]
        public void TearDown()
        {
            _carried.Terminate();
            _world.Dispose();
        }

        [Test]
        public void PropDroppedWhileSpawnedLiesInTheHands()
        {
            _carried.Terminate();

            Assert.That(_body.enabled, Is.True);
            Assert.That(ShapesAt(Hands), Is.EqualTo(1));
            Assert.That(_releaser.TryGetReleaser(out var releaser), Is.True);
            Assert.That(releaser, Is.SameAs(_holder));
        }

        [Test]
        public void PropDroppedAndThenDespawnedLeavesNothingInTheWorld()
        {
            _carried.Terminate();
            _networkObject.Despawn();

            Assert.That(ShapesAt(Hands), Is.Zero);
        }

        [Test]
        public void PropDespawnedInTheHandsLeavesNothingInTheWorldWhenTheCarryEndsAfter()
        {
            _networkObject.Despawn();
            _carried.Terminate();

            Assert.That(ShapesAt(Hands), Is.Zero, "a client learns of a broken crate's despawn before its rollback takes the crate out of the hands, and the holder runs into whatever is left");
        }

        [Test]
        public void PropSpawnedAgainHasNoReleaser()
        {
            _carried.Terminate();
            _networkObject.Despawn();
            _networkObject.Spawn();

            Assert.That(_releaser.TryGetReleaser(out _), Is.False, "a pooled crate spawned again is nobody's throw");
        }

        [Test]
        public void CarryEndingInAReplayWithoutAThrowIsNoRelease()
        {
            _worldStep.Stepped(TickPhase.Default).Advise(_world.Lifetime, _ => _carried.Terminate());

            _worldStep.Replay(5);

            Assert.That(ShapesAt(Hands), Is.Zero, "a rewind that takes the crate out of the hands is no drop: the crate's own state says where it is");
            Assert.That(_releaser.TryGetReleaser(out _), Is.False);
        }

        [Test]
        public void ThrowInAReplayIsReleasedAgain()
        {
            _worldStep.Stepped(TickPhase.Default).Advise(_world.Lifetime, _ =>
            {
                _prop.Fire(0f);
                _carried.Terminate();
            });

            _worldStep.Replay(5);

            Assert.That(ShapesAt(Hands), Is.EqualTo(1), "a replayed throw is simulated again in its tick");
            Assert.That(_releaser.TryGetReleaser(out var releaser), Is.True);
            Assert.That(releaser, Is.SameAs(_holder));
        }

        private int ShapesAt(Vector2 position)
        {
            var filter = new PhysicsQuery.QueryFilter(PhysicsMask.All, PhysicsMask.All);
            using var found = _world.World.OverlapGeometry(PolygonGeometry.CreateBox(Size * 0.5f, 0f, new PhysicsTransform(position), false), filter, Allocator.Temp);
            return found.Length;
        }
    }
}
