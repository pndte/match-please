using System;
using System.Collections.Generic;
using Bw.Tests.Doubles;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Melee;
using Bw.UseCases.Combat.Melee.Extensions;
using NUnit.Framework;
using UnityEngine;
using static Bw.Tests.UseCases.Combat.Melee.Doubles.EighthSecondFists;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Combat.Melee
{
    public sealed class MeleeSimulatorTests
    {
        private MeleeWeaponConfig _config;
        private MeleeSimulator _simulator;

        [SetUp]
        public void SetUp()
        {
            _config = Create();
            _simulator = new MeleeSimulator(new ManualClock(Step), _config);
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [Test]
        public void PressStartsAJabHeldByTheButton()
        {
            var state = _simulator.Step(default, Input(true));

            Assert.That(state.Phase, Is.EqualTo(MeleePhase.Jab));
            Assert.That(state.PhaseTicks, Is.Zero);
            Assert.That(state.Held, Is.True);
            Assert.That(state.Shots, Is.Zero);
        }

        [Test]
        public void IdleFistsWithoutAPullStayIdle()
        {
            var state = _simulator.Step(default, Input(false));

            Assert.That(state, Is.EqualTo(default(MeleeState)));
        }

        [Test]
        public void ClickedJabStrikesOnceOnItsStrikeTick()
        {
            var states = Run(tick => tick == 0, 20);

            Assert.That(states[StrikeTicks - 1].Shots, Is.Zero);
            Assert.That(states[StrikeTicks].Shots, Is.EqualTo(1));
            Assert.That(states[19].Shots, Is.EqualTo(1));
        }

        [Test]
        public void ReleaseDuringTheJabDropsTheHold()
        {
            var states = Run(tick => tick == 0, 2);

            Assert.That(states[1].Phase, Is.EqualTo(MeleePhase.Jab));
            Assert.That(states[1].Held, Is.False);
        }

        [Test]
        public void ClickedJabEndsIdleAfterItsSwing()
        {
            var states = Run(tick => tick == 0, 20);

            Assert.That(states[SwingTicks - 1].Phase, Is.EqualTo(MeleePhase.Jab));
            Assert.That(states[SwingTicks].Phase, Is.EqualTo(MeleePhase.Idle));
            Assert.That(states.FindAll(state => state.Phase == MeleePhase.Charging), Is.Empty);
        }

        [Test]
        public void JabHeldToItsEndStartsTheCharge()
        {
            var states = Run(tick => tick <= SwingTicks, SwingTicks + 1);

            Assert.That(states[SwingTicks - 1].Held, Is.True);
            Assert.That(states[SwingTicks].Phase, Is.EqualTo(MeleePhase.Charging));
            Assert.That(states[SwingTicks].ChargeTicks, Is.Zero);
            Assert.That(states[SwingTicks].Held, Is.False);
        }

        [Test]
        public void ReleaseBeforeTheMinimumChargeFiresAtTheMinimum()
        {
            var states = Run(tick => tick <= SwingTicks, 30);

            Assert.That(FirstStrongStart(states), Is.EqualTo(SwingTicks + MinChargeTicks));
            Assert.That(states[SwingTicks + MinChargeTicks].ChargeTicks, Is.EqualTo(MinChargeTicks));
        }

        [Test]
        public void ReleaseAfterTheMinimumChargeFiresOnTheRelease()
        {
            var release = SwingTicks + MinChargeTicks + 2;

            var states = Run(tick => tick < release, 30);

            Assert.That(FirstStrongStart(states), Is.EqualTo(release));
            Assert.That(states[release].ChargeTicks, Is.EqualTo(MinChargeTicks + 2));
        }

        [Test]
        public void ChargeHeldPastItsPeakFiresByItselfAfterTheHold()
        {
            var states = Run(_ => true, 30);

            Assert.That(FirstStrongStart(states), Is.EqualTo(SwingTicks + FullChargeTicks + FullChargeHoldTicks));
        }

        [Test]
        public void PressAgainBeforeTheMinimumKeepsCharging()
        {
            var states = Run(tick => tick <= SwingTicks || tick >= SwingTicks + 2, 30);

            Assert.That(FirstStrongStart(states), Is.EqualTo(SwingTicks + FullChargeTicks + FullChargeHoldTicks));
        }

        [Test]
        public void StrongSwingStrikesOnItsStrikeTickWithItsCharge()
        {
            var start = SwingTicks + MinChargeTicks;

            var states = Run(tick => tick <= SwingTicks, 30);

            Assert.That(states[start + StrongStrikeTicks - 1].Shots, Is.EqualTo(1));
            Assert.That(states[start + StrongStrikeTicks].Shots, Is.EqualTo(2));
            Assert.That(states[start + StrongStrikeTicks].ChargeTicks, Is.EqualTo(MinChargeTicks));
        }

        [Test]
        public void StrongSwingIsFollowedByTheRecoveryAndThenIdle()
        {
            var start = SwingTicks + MinChargeTicks;

            var states = Run(tick => tick <= SwingTicks, 30);

            Assert.That(states[start + StrongSwingTicks - 1].Phase, Is.EqualTo(MeleePhase.StrongSwing));
            Assert.That(states[start + StrongSwingTicks].Phase, Is.EqualTo(MeleePhase.Recovery));
            Assert.That(states[start + StrongSwingTicks + RecoveryTicks - 1].Phase, Is.EqualTo(MeleePhase.Recovery));
            Assert.That(states[start + StrongSwingTicks + RecoveryTicks].Phase, Is.EqualTo(MeleePhase.Idle));
            Assert.That(states[start + StrongSwingTicks + RecoveryTicks].ChargeTicks, Is.Zero);
            Assert.That(states[29].Shots, Is.EqualTo(2));
        }

        [Test]
        public void TriggerStillHeldAfterTheRecoveryStartsTheNextJab()
        {
            var end = SwingTicks + FullChargeTicks + FullChargeHoldTicks + StrongSwingTicks + RecoveryTicks;

            var states = Run(_ => true, end + 1);

            Assert.That(states[end].Phase, Is.EqualTo(MeleePhase.Jab));
            Assert.That(states[end].PhaseTicks, Is.Zero);
        }

        [Test]
        public void PullDuringTheRecoveryDoesNotJab()
        {
            var state = _simulator.Step(Melee(MeleePhase.Recovery, 0), Input(true));

            Assert.That(state.Phase, Is.EqualTo(MeleePhase.Recovery));
            Assert.That(state.PhaseTicks, Is.EqualTo(1));
        }

        [Test]
        public void WithoutARecoveryTheStrongSwingEndsInIdle()
        {
            _config.RecoveryTime = 0f;

            var state = _simulator.Step(Melee(MeleePhase.StrongSwing, StrongSwingTicks - 1), Input(false));

            Assert.That(state.Phase, Is.EqualTo(MeleePhase.Idle));
        }

        [Test]
        public void ClicksOnEveryJabEndJabAgainWithoutCharging()
        {
            var states = Run(tick => tick % SwingTicks == 0, 3 * SwingTicks + 1);

            Assert.That(states[3 * SwingTicks].Phase, Is.EqualTo(MeleePhase.Jab));
            Assert.That(states[3 * SwingTicks].PhaseTicks, Is.Zero);
            Assert.That(states[3 * SwingTicks].Shots, Is.EqualTo(3));
            Assert.That(states.FindAll(state => state.Phase == MeleePhase.Charging), Is.Empty);
        }

        [Test]
        public void PressAgainInsideTheJabCountsOnlyFromTheNextJab()
        {
            var states = Run(tick => tick == 0 || tick >= 2, 2 * SwingTicks + 1);

            Assert.That(states[2].Held, Is.False);
            Assert.That(states[SwingTicks].Phase, Is.EqualTo(MeleePhase.Jab));
            Assert.That(states[SwingTicks].PhaseTicks, Is.Zero);
            Assert.That(states[SwingTicks].Held, Is.True);
            Assert.That(states[2 * SwingTicks].Phase, Is.EqualTo(MeleePhase.Charging));
        }

        [TestCase(MeleePhase.Idle)]
        [TestCase(MeleePhase.Jab)]
        [TestCase(MeleePhase.Charging)]
        public void InterruptionEndsInIdleKeepingTheAimAndTheCount(MeleePhase phase)
        {
            var state = new MeleeState(30f, phase, 3, true, 5, 7);

            var interrupted = _simulator.Interrupted(state);

            Assert.That(interrupted, Is.EqualTo(new MeleeState(30f, MeleePhase.Idle, 0, false, 0, 7)));
        }

        [Test]
        public void InterruptionCutsAStrongSwingIntoTheRecovery()
        {
            var interrupted = _simulator.Interrupted(new MeleeState(30f, MeleePhase.StrongSwing, 1, false, 6, 1));

            Assert.That(interrupted, Is.EqualTo(new MeleeState(30f, MeleePhase.Recovery, 0, false, 0, 1)));
        }

        [Test]
        public void InterruptionWithoutARecoveryEndsAStrongSwingInIdle()
        {
            _config.RecoveryTime = 0f;

            var interrupted = _simulator.Interrupted(Melee(MeleePhase.StrongSwing, 1));

            Assert.That(interrupted.Phase, Is.EqualTo(MeleePhase.Idle));
        }

        [Test]
        public void InterruptionLeavesARecoveryAlone()
        {
            var recovering = Melee(MeleePhase.Recovery, 1);

            Assert.That(_simulator.Interrupted(recovering), Is.EqualTo(recovering));
        }

        [Test]
        public void RestLetsTheRecoveryPass()
        {
            var first = _simulator.Rested(Melee(MeleePhase.Recovery, 0));
            var last = _simulator.Rested(first);

            Assert.That(first, Is.EqualTo(Melee(MeleePhase.Recovery, 1)));
            Assert.That(last.Phase, Is.EqualTo(MeleePhase.Idle));
        }

        [Test]
        public void RestOutOfHandsEndsAChargeLikeAnInterruption()
        {
            var charging = new MeleeState(0f, MeleePhase.Charging, 3, false, 3, 1);

            Assert.That(_simulator.Rested(charging), Is.EqualTo(_simulator.Interrupted(charging)));
        }

        [Test]
        public void AimTurnsTowardsTheInputAtTheTurnSpeed()
        {
            var first = _simulator.Step(default, Aimed(25f));
            var second = _simulator.Step(first, Aimed(25f));
            var third = _simulator.Step(second, Aimed(25f));

            Assert.That(first.Aim, Is.EqualTo(TurnPerTick).Within(1e-4f));
            Assert.That(second.Aim, Is.EqualTo(2f * TurnPerTick).Within(1e-4f));
            Assert.That(third.Aim, Is.EqualTo(25f).Within(1e-4f));
        }

        [Test]
        public void AimTurnsTheShortWayAcrossTheBack()
        {
            var state = _simulator.Step(new MeleeState(170f, MeleePhase.Idle, 0, false, 0, 0), Aimed(-170f));

            Assert.That(Mathf.DeltaAngle(state.Aim, 180f), Is.EqualTo(0f).Within(1e-4f));
        }

        [Test]
        public void InterruptionDoesNotTurn()
        {
            var interrupted = _simulator.Interrupted(new MeleeState(30f, MeleePhase.Jab, 1, true, 0, 0));

            Assert.That(interrupted.Aim, Is.EqualTo(30f));
        }

        [Test]
        public void UnknownPhaseThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _simulator.Step(Melee((MeleePhase)9, 0), Input(false)));
        }

        [Test]
        public void StepFromAStoredStateRepeatsTheSameStates()
        {
            var once = Run(_ => true, 30);

            var again = Run(_ => true, 15, once[14]);

            Assert.That(again[14], Is.EqualTo(once[29]));
        }

        [Test]
        public void StrongSwingIsSeenStartingExactlyOnce()
        {
            var states = Run(_ => true, 30);

            Assert.That(states.FindAll(state => state.StrongSwingStarted()), Has.Count.EqualTo(1));
        }

        private List<MeleeState> Run(Func<int, bool> pulled, int ticks, MeleeState start = default)
        {
            var states = new List<MeleeState>();
            var state = start;
            for (var tick = 0; tick < ticks; tick++)
            {
                state = _simulator.Step(state, Input(pulled(tick)));
                states.Add(state);
            }

            return states;
        }

        private static int FirstStrongStart(List<MeleeState> states) =>
            states.FindIndex(state => state.StrongSwingStarted());

        private static MeleeState Melee(MeleePhase phase, int phaseTicks) =>
            new(0f, phase, phaseTicks, false, 0, 0);

        private static WeaponInput Input(bool pulled) =>
            new(0f, pulled, false, false);

        private static WeaponInput Aimed(float aim) =>
            new(aim, false, false, false);
    }
}
