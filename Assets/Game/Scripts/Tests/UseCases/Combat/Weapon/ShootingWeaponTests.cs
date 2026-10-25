using System;
using System.Collections.Generic;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Shooting.Extensions;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon
{
    public sealed class ShootingWeaponTests
    {
        private static readonly GunState Initial = new(0f, 3, 2, 0, 4);

        private LifetimeDefinition _lifetime;
        private ShootingWeapon<GunState> _weapon;
        private List<GunState> _stepped;
        private List<WeaponShot> _fired;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _weapon = new ShootingWeapon<GunState>(new ScriptedSimulator(), Initial);
            _stepped = new List<GunState>();
            _fired = new List<WeaponShot>();
            _weapon.Stepped.Advise(_lifetime.Lifetime, _stepped.Add);
            _weapon.Fired.Advise(_lifetime.Lifetime, _fired.Add);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void WeaponStartsInTheGivenState()
        {
            Assert.That(_weapon.State.Value, Is.EqualTo(Initial));
            Assert.That(_weapon.Previous, Is.EqualTo(Initial));
        }

        [Test]
        public void StepFeedsTheInputBeforeTheStateMoves()
        {
            var fed = new List<(WeaponInput input, GunState state)>();
            _weapon.Fed.Advise(_lifetime.Lifetime, input => fed.Add((input, _weapon.State.Value)));

            _weapon.Step(new WeaponInput(15f, false, true, false));

            Assert.That(fed, Has.Count.EqualTo(1));
            Assert.That(fed[0].input.Aim, Is.EqualTo(15f));
            Assert.That(fed[0].input.ReloadPressed, Is.True);
            Assert.That(fed[0].state, Is.EqualTo(Initial));
        }

        [Test]
        public void StepMovesTheStateByTheSimulatorAndRemembersThePrevious()
        {
            _weapon.Step(new WeaponInput(15f, false, false, false));

            Assert.That(_weapon.State.Value, Is.EqualTo(new GunState(15f, 3, 2, 0, 4)));
            Assert.That(_weapon.Previous, Is.EqualTo(Initial));
            Assert.That(_stepped, Is.EqualTo(new[] { new GunState(15f, 3, 2, 0, 4) }));
        }

        [Test]
        public void StepThatAddsAShotFiresItAlongTheNewAim()
        {
            _weapon.Step(new WeaponInput(40f, true, false, false));

            Assert.That(_fired, Has.Count.EqualTo(1));
            Assert.That(_fired[0].Aim, Is.EqualTo(40f));
        }

        [Test]
        public void ShotIsFiredAfterTheStateIsStepped()
        {
            var shotState = new List<GunState>();
            _weapon.Fired.Advise(_lifetime.Lifetime, _ => shotState.Add(_weapon.State.Value));

            _weapon.Step(new WeaponInput(40f, true, false, false));

            Assert.That(shotState, Is.EqualTo(new[] { new GunState(40f, 3, 2, 0, 5) }));
        }

        [Test]
        public void StepWithoutAShotFiresNothing()
        {
            _weapon.Step(new WeaponInput(40f, false, false, false));

            Assert.That(_fired, Is.Empty);
        }

        [Test]
        public void EveryStepThatAddsAShotFiresOnce()
        {
            for (var tick = 0; tick < 4; tick++)
                _weapon.Step(new WeaponInput(tick, true, false, false));

            Assert.That(_fired, Has.Count.EqualTo(4));
        }

        [Test]
        public void RestThatChangesTheStateStepsIt()
        {
            _weapon.Step();

            Assert.That(_weapon.State.Value, Is.EqualTo(new GunState(0f, 3, 1, 0, 4)));
            Assert.That(_weapon.Previous, Is.EqualTo(Initial));
            Assert.That(_stepped, Is.EqualTo(new[] { new GunState(0f, 3, 1, 0, 4) }));
        }

        [Test]
        public void RestThatChangesNothingStepsNothing()
        {
            var cool = new GunState(0f, 3, 0, 0, 4);
            _weapon.Apply(cool);

            _weapon.Step();

            Assert.That(_stepped, Is.Empty);
            Assert.That(_weapon.State.Value, Is.EqualTo(cool));
            Assert.That(_weapon.Previous, Is.EqualTo(cool));
        }

        [Test]
        public void RestNeverFeedsNorFires()
        {
            var fed = 0;
            _weapon.Fed.Advise(_lifetime.Lifetime, _ => fed++);

            _weapon.Step();

            Assert.That(fed, Is.EqualTo(0));
            Assert.That(_fired, Is.Empty);
        }

        [Test]
        public void AppliedStateIsTakenWithoutSteppingOrFiring()
        {
            var applied = new GunState(90f, 1, 0, 3, 20);

            _weapon.Apply(applied);

            Assert.That(_weapon.State.Value, Is.EqualTo(applied));
            Assert.That(_weapon.Previous, Is.EqualTo(Initial));
            Assert.That(_stepped, Is.Empty);
            Assert.That(_fired, Is.Empty);
        }

        [Test]
        public void StepAfterAnAppliedStateFiresOnlyItsOwnShot()
        {
            _weapon.Apply(new GunState(0f, 3, 0, 0, 20));

            _weapon.Step(new WeaponInput(0f, false, false, false));

            Assert.That(_fired, Is.Empty);
        }

        [Test]
        public void StowingAndRootingFollowTheState()
        {
            var weapon = new ShootingWeapon<ToyState>(new ToySimulator(), new ToyState(true, false, 0));

            weapon.Apply(new ToyState(false, true, 0));

            Assert.That(weapon.Stowable, Is.False);
            Assert.That(weapon.Rooting, Is.True);
        }

        private sealed class ScriptedSimulator : IWeaponSimulator<GunState>
        {
            public GunState Step(GunState state, WeaponInput input) =>
                new(input.Aim, state.Ammo, state.CooldownTicks, state.ReloadTicks, input.Pulled ? state.Shots + 1 : state.Shots);

            public GunState Interrupted(GunState state) =>
                state.WithoutReload();

            public GunState Rested(GunState state) =>
                state.CooledDown();
        }

        private readonly struct ToyState : IWeaponState, IEquatable<ToyState>
        {
            public float Aim => 0f;
            public int Shots { get; }
            public bool Stowable { get; }
            public bool Rooting { get; }

            public ToyState(bool stowable, bool rooting, int shots)
            {
                Stowable = stowable;
                Rooting = rooting;
                Shots = shots;
            }

            public bool Equals(ToyState other) =>
                Stowable == other.Stowable && Rooting == other.Rooting && Shots == other.Shots;
        }

        private sealed class ToySimulator : IWeaponSimulator<ToyState>
        {
            public ToyState Step(ToyState state, WeaponInput input) =>
                state;

            public ToyState Interrupted(ToyState state) =>
                state;

            public ToyState Rested(ToyState state) =>
                state;
        }
    }
}
