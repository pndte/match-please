using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Shooting.Extensions;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Mortality;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class RaycastShooterTests
    {
        private static readonly GunState Current = new(0f, 2, 3, 0, 5);

        private LifetimeDefinition _lifetime;
        private FakeWeapon _weapon;
        private FakeFeed _feed;
        private FakeCharacter _shooter;
        private CharacterTrigger _trigger;
        private AimedTracer _tracer;
        private RecordingPower _power;
        private RecordingShots _shots;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _weapon = new FakeWeapon();
            _feed = new FakeFeed();
            _shooter = new FakeCharacter();
            _trigger = new CharacterTrigger();
            var carriers = new WeaponCarriers();
            carriers.Add(_lifetime.Lifetime, _weapon, _shooter);
            var triggers = new CharacterTriggers();
            triggers.ByCharacter.AddLifetimed(_lifetime.Lifetime, _shooter, _trigger);
            var timer = new ShotTimer(_lifetime.Lifetime, _weapon, _feed, carriers, triggers);
            var simulation = new ShootingWeapon<GunState>(new IdleSimulator(), Current);
            _tracer = new AimedTracer();
            _power = new RecordingPower();
            _shots = new RecordingShots();
            new RaycastShooter<GunState>(_lifetime.Lifetime, _weapon, simulation, _tracer, _shots, _power, timer);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void FiredShotIsSubmittedWithItsTimingAndShooter()
        {
            _trigger.Take(new TriggerControls(true, 5f, 2f, new TriggerPress(1, 0, 0f, 0f, 1)));
            _feed.Feed();

            _weapon.Fire(90f);

            Assert.That(_shots.Submitted, Has.Count.EqualTo(1));
            var shot = _shots.Submitted[0];
            Assert.That(shot.Tracer, Is.SameAs(_tracer));
            Assert.That(shot.Weapon, Is.SameAs(_weapon));
            Assert.That(shot.Shooter, Is.SameAs(_shooter));
            Assert.That(shot.WeaponShot.Aim, Is.EqualTo(90f));
            Assert.That(shot.Timing.ViewDelay, Is.EqualTo(5f));
            Assert.That(shot.Timing.PredictedDelay, Is.EqualTo(2f));
            Assert.That(shot.Timing.LateTicks, Is.EqualTo(1));
        }

        [Test]
        public void HitIsTheWeaponsPowerAlongTheAimedRayInTheCurrentState()
        {
            _trigger.Take(new TriggerControls(true, 0f, 0f, default));
            _feed.Feed();

            _weapon.Fire(90f);

            Assert.That(_power.Directions, Has.Count.EqualTo(1));
            Assert.That(_power.Directions[0].x, Is.EqualTo(0f).Within(1e-5f));
            Assert.That(_power.Directions[0].y, Is.EqualTo(1f).Within(1e-5f));
            Assert.That(_power.States, Is.EqualTo(new[] { Current }));
            Assert.That(_shots.Submitted[0].Hit.Damage, Is.EqualTo(RecordingPower.Damage));
        }

        [Test]
        public void ShotsAfterTheShootersLifetimeAreNotSubmitted()
        {
            _trigger.Take(new TriggerControls(true, 0f, 0f, default));
            _feed.Feed();
            _lifetime.Terminate();

            _weapon.Fire(0f);

            Assert.That(_shots.Submitted, Is.Empty);
        }

        private sealed class IdleSimulator : IWeaponSimulator<GunState>
        {
            public GunState Step(GunState state, WeaponInput input) =>
                state;

            public GunState Interrupted(GunState state) =>
                state;

            public GunState Rested(GunState state) =>
                state;
        }

        private sealed class AimedTracer : IShotTracer
        {
            public int Pellets => 1;

            public ShotRay Aim(float aim) =>
                new ShotRay(Vector2.zero, Vector2.right, Vector2.right).Turned(aim);

            public ShotRay Pellet(ShotRay ray, int pellet) =>
                ray;

            public ShotTrace Cast(ShotRay ray) =>
                ShotTrace.Missed(ray.Muzzle, ray.Muzzle + ray.Direction);
        }

        private sealed class RecordingPower : IShotPower<GunState>
        {
            public const float Damage = 17f;

            public List<Vector2> Directions { get; } = new();
            public List<GunState> States { get; } = new();

            public Hit Along(Vector2 direction, GunState state)
            {
                Directions.Add(direction);
                States.Add(state);
                return new Hit(Damage, direction);
            }
        }

        private sealed class RecordingShots : IRaycastShots
        {
            public List<RaycastShot> Submitted { get; } = new();

            public void Submit(RaycastShot shot) =>
                Submitted.Add(shot);
        }
    }
}
