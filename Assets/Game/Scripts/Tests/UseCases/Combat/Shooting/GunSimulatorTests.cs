using System.Collections.Generic;
using Bw.Tests.Doubles;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Shooting;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Shooting
{
    public sealed class GunSimulatorTests
    {
        private const float TickSeconds = 0.125f;
        private const int CooldownTicks = 4;
        private const int ReloadTicks = 8;
        private const int Magazine = 3;

        private AmmoConfig _ammo;
        private ShootingWeaponConfig _config;
        private GunSimulator _gun;

        [SetUp]
        public void SetUp()
        {
            _ammo = ScriptableObject.CreateInstance<AmmoConfig>();
            _ammo.Max = Magazine;
            _ammo.OnSpawnValue = Magazine;
            _config = ScriptableObject.CreateInstance<ShootingWeaponConfig>();
            _config.AmmoSettings = _ammo;
            _config.ShootCooldown = CooldownTicks * TickSeconds;
            _config.ReloadTime = ReloadTicks * TickSeconds;
            _config.TurnSpeed = 80f;
            _gun = new GunSimulator(new ManualClock(TickSeconds), _config);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_ammo);
        }

        [Test]
        public void PulledTriggerOfAReadyGunFiresAtOnce()
        {
            var state = _gun.Step(Loaded(Magazine), Pull());

            Assert.That(state.Shots, Is.EqualTo(1));
            Assert.That(state.Ammo, Is.EqualTo(Magazine - 1));
            Assert.That(state.CooldownTicks, Is.EqualTo(CooldownTicks));
            Assert.That(state.ReloadTicks, Is.EqualTo(0));
        }

        [Test]
        public void ReleasedTriggerNeverFires()
        {
            var state = Loaded(Magazine);
            for (var tick = 0; tick < 20; tick++)
                state = _gun.Step(state, Idle());

            Assert.That(state.Shots, Is.EqualTo(0));
            Assert.That(state.Ammo, Is.EqualTo(Magazine));
        }

        [Test]
        public void HeldTriggerFiresOncePerCooldown()
        {
            _ammo.Max = 10;

            var shots = ShotTicks(Loaded(10), Pull(), 13, out _);

            Assert.That(shots, Is.EqualTo(new[] { 0, 4, 8, 12 }));
        }

        [TestCase(0.5f, 4)]
        [TestCase(0.3f, 3)]
        [TestCase(0.125f, 1)]
        [TestCase(0.01f, 1)]
        [TestCase(0f, 0)]
        public void CooldownIsTheSecondsRoundedUpToWholeTicks(float seconds, int ticks)
        {
            _config.ShootCooldown = seconds;

            var state = _gun.Step(Loaded(Magazine), Pull());

            Assert.That(state.CooldownTicks, Is.EqualTo(ticks));
        }

        [Test]
        public void GunWithoutCooldownFiresEveryTick()
        {
            _config.ShootCooldown = 0f;
            _ammo.Max = 10;

            var shots = ShotTicks(Loaded(10), Pull(), 5, out _);

            Assert.That(shots, Is.EqualTo(new[] { 0, 1, 2, 3, 4 }));
        }

        [Test]
        public void TriggerPulledDuringTheCooldownWaitsForItsEnd()
        {
            var cooling = new GunState(0f, Magazine, 2, 0, 0);

            var waiting = _gun.Step(cooling, Pull());
            var fired = _gun.Step(waiting, Pull());

            Assert.That(waiting.Shots, Is.EqualTo(0));
            Assert.That(waiting.CooldownTicks, Is.EqualTo(1));
            Assert.That(fired.Shots, Is.EqualTo(1));
            Assert.That(fired.CooldownTicks, Is.EqualTo(CooldownTicks));
        }

        [Test]
        public void CooldownRunsDownWithoutTheTrigger()
        {
            var state = _gun.Step(new GunState(0f, Magazine, 3, 0, 0), Idle());

            Assert.That(state.CooldownTicks, Is.EqualTo(2));
        }

        [Test]
        public void PulledEmptyGunStartsAReloadInsteadOfFiring()
        {
            var state = _gun.Step(Loaded(0), Pull());

            Assert.That(state.Shots, Is.EqualTo(0));
            Assert.That(state.Ammo, Is.EqualTo(0));
            Assert.That(state.ReloadTicks, Is.EqualTo(ReloadTicks));
        }

        [Test]
        public void ReloadPressedWithMissingRoundsStartsAReload()
        {
            var state = _gun.Step(Loaded(1), Reload());

            Assert.That(state.ReloadTicks, Is.EqualTo(ReloadTicks));
            Assert.That(state.Ammo, Is.EqualTo(1));
        }

        [Test]
        public void ReloadPressedWithAFullMagazineIsIgnored()
        {
            var state = _gun.Step(Loaded(Magazine), Reload());

            Assert.That(state.ReloadTicks, Is.EqualTo(0));
        }

        [Test]
        public void ReloadPressTakesPrecedenceOverThePull()
        {
            var state = _gun.Step(Loaded(1), new WeaponInput(0f, true, true, false));

            Assert.That(state.ReloadTicks, Is.EqualTo(ReloadTicks));
            Assert.That(state.Shots, Is.EqualTo(0));
            Assert.That(state.Ammo, Is.EqualTo(1));
        }

        [Test]
        public void ReloadFillsTheMagazineWhenItsTicksRunOut()
        {
            var state = _gun.Step(Loaded(0), Reload());
            for (var tick = 1; tick < ReloadTicks; tick++)
                state = _gun.Step(state, Idle());

            var lastTick = state;
            state = _gun.Step(state, Idle());

            Assert.That(lastTick.ReloadTicks, Is.EqualTo(1));
            Assert.That(lastTick.Ammo, Is.EqualTo(0));
            Assert.That(state.ReloadTicks, Is.EqualTo(0));
            Assert.That(state.Ammo, Is.EqualTo(Magazine));
        }

        [Test]
        public void GunNeverFiresWhileReloading()
        {
            var state = _gun.Step(Loaded(1), Reload());
            var shots = state.Shots;
            for (var tick = 1; tick < ReloadTicks; tick++)
                state = _gun.Step(state, Pull());

            Assert.That(state.Shots, Is.EqualTo(shots));
            Assert.That(state.Ammo, Is.EqualTo(1));
        }

        [Test]
        public void TriggerHeldThroughAReloadFiresOnTheTickTheMagazineIsFull()
        {
            var shots = ShotTicks(Loaded(0), Pull(), ReloadTicks + 2, out var state);

            Assert.That(shots, Is.EqualTo(new[] { ReloadTicks }));
            Assert.That(state.Ammo, Is.EqualTo(Magazine - 1));
        }

        [Test]
        public void ReloadPressedDuringAReloadDoesNotRestartIt()
        {
            var state = _gun.Step(Loaded(0), Reload());
            state = _gun.Step(state, Idle());

            state = _gun.Step(state, Reload());

            Assert.That(state.ReloadTicks, Is.EqualTo(ReloadTicks - 2));
        }

        [Test]
        public void InstantReloadStillTakesOneTick()
        {
            _config.ReloadTime = 0f;

            var started = _gun.Step(Loaded(0), Reload());
            var done = _gun.Step(started, Idle());

            Assert.That(started.ReloadTicks, Is.EqualTo(1));
            Assert.That(started.Ammo, Is.EqualTo(0));
            Assert.That(done.ReloadTicks, Is.EqualTo(0));
            Assert.That(done.Ammo, Is.EqualTo(Magazine));
        }

        [Test]
        public void LastRoundLeavesTheGunEmptyUntilTheTriggerIsPulledAgain()
        {
            var fired = _gun.Step(Loaded(1), Pull());
            var idle = fired;
            for (var tick = 0; tick < CooldownTicks + 2; tick++)
                idle = _gun.Step(idle, Idle());

            var pulled = _gun.Step(idle, Pull());

            Assert.That(fired.Ammo, Is.EqualTo(0));
            Assert.That(fired.ReloadTicks, Is.EqualTo(0));
            Assert.That(idle.ReloadTicks, Is.EqualTo(0));
            Assert.That(pulled.ReloadTicks, Is.EqualTo(ReloadTicks));
        }

        [Test]
        public void EmptyGunStartsItsReloadEvenDuringTheCooldown()
        {
            var state = _gun.Step(new GunState(0f, 0, 3, 0, 5), Pull());

            Assert.That(state.ReloadTicks, Is.EqualTo(ReloadTicks));
            Assert.That(state.CooldownTicks, Is.EqualTo(2));
        }

        [Test]
        public void MagazineEmptiedByAHeldTriggerReloadsAndFiresAgain()
        {
            var shots = ShotTicks(Loaded(Magazine), Pull(), 20, out var state);

            Assert.That(shots, Is.EqualTo(new[] { 0, 4, 8, 17 }));
            Assert.That(state.Ammo, Is.EqualTo(Magazine - 1));
        }

        [Test]
        public void ShotCounterCountsEveryShot()
        {
            _ammo.Max = 10;

            ShotTicks(Loaded(10), Pull(), 9, out var state);

            Assert.That(state.Shots, Is.EqualTo(3));
            Assert.That(state.Ammo, Is.EqualTo(7));
        }

        [Test]
        public void AimTurnsTowardTheInputAtTheTurnSpeed()
        {
            var state = Loaded(Magazine);
            var aims = new List<float>();
            for (var tick = 0; tick < 6; tick++)
            {
                state = _gun.Step(state, new WeaponInput(45f, false, false, false));
                aims.Add(state.Aim);
            }

            Assert.That(aims, Is.EqualTo(new[] { 10f, 20f, 30f, 40f, 45f, 45f }).Within(1e-4f));
        }

        [Test]
        public void AimTurnsTheShortWayAcrossTheBackOfTheCircle()
        {
            var state = new GunState(170f, Magazine, 0, 0, 0);

            var first = _gun.Step(state, new WeaponInput(-170f, false, false, false));
            var second = _gun.Step(first, new WeaponInput(-170f, false, false, false));

            Assert.That(Mathf.Abs(first.Aim), Is.EqualTo(180f).Within(1e-4f));
            Assert.That(second.Aim, Is.EqualTo(-170f).Within(1e-4f));
        }

        [Test]
        public void AimTurnsAsItFires()
        {
            var state = _gun.Step(Loaded(Magazine), new WeaponInput(90f, true, false, false));

            Assert.That(state.Shots, Is.EqualTo(1));
            Assert.That(state.Aim, Is.EqualTo(10f).Within(1e-4f));
        }

        [Test]
        public void SameStateAndInputGiveTheSameNextState()
        {
            var state = new GunState(12f, 2, 1, 0, 9);
            var input = new WeaponInput(-30f, true, false, false);

            Assert.That(_gun.Step(state, input), Is.EqualTo(_gun.Step(state, input)));
        }

        [Test]
        public void InterruptionCancelsTheReloadOnly()
        {
            var interrupted = _gun.Interrupted(new GunState(10f, 2, 3, 5, 7));

            Assert.That(interrupted, Is.EqualTo(new GunState(10f, 2, 3, 0, 7)));
        }

        [Test]
        public void RestCancelsTheReloadAndCoolsDownOneTick()
        {
            var rested = _gun.Rested(new GunState(10f, 2, 3, 5, 7));

            Assert.That(rested, Is.EqualTo(new GunState(10f, 2, 2, 0, 7)));
        }

        [Test]
        public void RestOfACoolGunChangesNothing()
        {
            var cool = new GunState(10f, 2, 0, 0, 7);

            Assert.That(_gun.Rested(cool), Is.EqualTo(cool));
        }

        private static WeaponInput Pull() =>
            new(0f, true, false, false);

        private static WeaponInput Idle() =>
            new(0f, false, false, false);

        private static WeaponInput Reload() =>
            new(0f, false, true, false);

        private static GunState Loaded(int ammo) =>
            new(0f, ammo, 0, 0, 0);

        private List<int> ShotTicks(
            GunState state,
            WeaponInput input,
            int ticks,
            out GunState last)
        {
            var shots = new List<int>();
            for (var tick = 0; tick < ticks; tick++)
            {
                var next = _gun.Step(state, input);
                if (next.Shots != state.Shots)
                    shots.Add(tick);

                state = next;
            }

            last = state;
            return shots;
        }
    }
}
