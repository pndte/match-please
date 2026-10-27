using System.Collections.Generic;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Shooting.Network;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Movement;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Combat.Shooting.Network
{
    public sealed class WeaponReleaserTests
    {
        private ShootingRange _range;
        private FakeNetworkObject _networkObject;
        private WeaponCarriers _carriers;
        private RecordingBody _body;
        private LifetimeDefinition _carried;

        [SetUp]
        public void SetUp()
        {
            _range = new ShootingRange();
            var box = _range.Box("Gun", new Vector2(-5f, 0.5f), new Vector2(1f, 0.25f), ShootingRange.Weapons, false);
            var physics = _range.Space.CreateBody(_range.Lifetime, box, PhysicsBody.BodyType.Kinematic);
            var spots = new WeaponReleaseSpots(new HeldWeaponPose(new AimedGun(), _range.Orbit(0.5f, 0.5f)), physics, box, _range.Config<WeaponReleaseConfig>());
            var weapon = new FakeWeapon();
            _networkObject = new FakeNetworkObject();
            _carriers = new WeaponCarriers();
            _body = new RecordingBody();
            new WeaponReleaser(_range.Lifetime, _networkObject, weapon, _carriers, spots, _body);
            _networkObject.Spawn();
            _carried = new LifetimeDefinition();
            _carriers.Add(_carried.Lifetime, weapon, new FakeCharacter());
        }

        [TearDown]
        public void TearDown()
        {
            _carried.Terminate();
            _range.Dispose();
        }

        [Test]
        public void WeaponDroppedWhileSpawnedIsReleased()
        {
            _carried.Terminate();

            Assert.That(_body.Restarts.Count, Is.EqualTo(1));
        }

        [Test]
        public void WeaponDespawnedInTheHandsIsNotReleasedWhenTheCarryEndsAfter()
        {
            _networkObject.Despawn();
            _carried.Terminate();

            Assert.That(_body.Restarts, Is.Empty, "a despawned weapon has no body in the world to release");
        }

        private sealed class AimedGun : IReadonlySimulation<GunState>
        {
            public IReadonlyProperty<GunState> State { get; } = new ViewableProperty<GunState>(new GunState(0f, 0, 0, 0, 0));
            public GunState Previous => State.Value;
            public ISource<GunState> Stepped { get; } = new Signal<GunState>();
        }

        private sealed class RecordingBody : IRestartable<RigidState>
        {
            public List<RigidState> Restarts { get; } = new();

            public void Restart(RigidState state) =>
                Restarts.Add(state);
        }
    }
}
