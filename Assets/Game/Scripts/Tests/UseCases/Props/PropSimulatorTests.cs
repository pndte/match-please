using System;
using Bw.Tests.Doubles;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Props;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Props
{
    public sealed class PropSimulatorTests
    {
        private static readonly PropState Holding = new(0f, PropPhase.Holding, 0, 0, false);

        private HeldPropConfig _config;
        private PropSimulator _simulator;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<HeldPropConfig>();
            _config.TurnSpeed = 480f;
            _config.WindUpTime = 0.133f;
            _simulator = new PropSimulator(new ManualClock(1f / 30f), _config);
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [Test]
        public void FreshPressStartsTheWindUpAtItsFirstTick()
        {
            var state = _simulator.Step(Holding, Pressed());

            Assert.That(state.Phase, Is.EqualTo(PropPhase.WindingUp));
            Assert.That(state.PhaseTicks, Is.EqualTo(1));
            Assert.That(state.Throws, Is.Zero);
            Assert.That(state.TriggerHeld, Is.True);
        }

        [Test]
        public void HeldTriggerStartsNoWindUp()
        {
            var held = new PropState(0f, PropPhase.Holding, 0, 2, true);

            var state = _simulator.Step(held, Pressed());

            Assert.That(state.Phase, Is.EqualTo(PropPhase.Holding));
            Assert.That(state.Throws, Is.EqualTo(2));
            Assert.That(state.TriggerHeld, Is.True);
        }

        [Test]
        public void ReleasedTriggerIsRememberedWhileHolding()
        {
            var held = new PropState(0f, PropPhase.Holding, 0, 0, true);

            var state = _simulator.Step(held, Released());

            Assert.That(state.Phase, Is.EqualTo(PropPhase.Holding));
            Assert.That(state.TriggerHeld, Is.False);
        }

        [Test]
        public void ThrowComesOnTheFifthStepAfterThePressEvenWithTheButtonReleased()
        {
            var state = _simulator.Step(Holding, Pressed());
            var steps = 1;
            while (state.Phase == PropPhase.WindingUp && steps < 20)
            {
                state = _simulator.Step(state, Released());
                steps++;
            }

            Assert.That(steps, Is.EqualTo(5));
            Assert.That(state.Phase, Is.EqualTo(PropPhase.Holding));
            Assert.That(state.PhaseTicks, Is.Zero);
            Assert.That(state.Throws, Is.EqualTo(1));
        }

        [Test]
        public void WindUpCountsItsTicks()
        {
            var state = _simulator.Step(Holding, Pressed());
            var ticks = new[] { state.PhaseTicks, 0, 0, 0 };
            for (var index = 1; index < ticks.Length; index++)
            {
                state = _simulator.Step(state, Pressed());
                ticks[index] = state.PhaseTicks;
            }

            Assert.That(ticks, Is.EqualTo(new[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void ButtonHeldThroughTheThrowStartsNoSecondWindUp()
        {
            var state = Holding;
            for (var step = 0; step < 5; step++)
                state = _simulator.Step(state, Pressed());

            state = _simulator.Step(state, Pressed());

            Assert.That(state.Phase, Is.EqualTo(PropPhase.Holding));
            Assert.That(state.Throws, Is.EqualTo(1));
        }

        [Test]
        public void ReleaseAndPressAfterAThrowWindsUpAgain()
        {
            var state = Holding;
            for (var step = 0; step < 5; step++)
                state = _simulator.Step(state, Pressed());

            state = _simulator.Step(state, Released());
            state = _simulator.Step(state, Pressed());

            Assert.That(state.Phase, Is.EqualTo(PropPhase.WindingUp));
            Assert.That(state.Throws, Is.EqualTo(1));
        }

        [Test]
        public void AimTurnsAtTheTurnSpeedWhileHolding()
        {
            var state = _simulator.Step(Holding, Released(90f));

            Assert.That(state.Aim, Is.EqualTo(16f).Within(1e-4f));
        }

        [Test]
        public void AimKeepsTurningDuringTheWindUp()
        {
            var state = _simulator.Step(_simulator.Step(Holding, Pressed(90f)), Released(90f));

            Assert.That(state.Phase, Is.EqualTo(PropPhase.WindingUp));
            Assert.That(state.Aim, Is.EqualTo(32f).Within(1e-4f));
        }

        [Test]
        public void AimReachesACloseTargetExactly()
        {
            var state = _simulator.Step(Holding, Released(10f));

            Assert.That(state.Aim, Is.EqualTo(10f).Within(1e-4f));
        }

        [Test]
        public void AimTurnsTheShortWayAcrossTheBackAndStaysWithinHalfATurn()
        {
            var state = new PropState(175f, PropPhase.Holding, 0, 0, false);

            var turned = _simulator.Step(state, Released(-175f));

            Assert.That(turned.Aim, Is.EqualTo(-175f).Within(1e-3f));
        }

        [Test]
        public void InterruptCancelsTheWindUpAndWaitsForARelease()
        {
            var windingUp = new PropState(30f, PropPhase.WindingUp, 3, 2, false);

            var interrupted = _simulator.Interrupted(windingUp);

            Assert.That(interrupted, Is.EqualTo(new PropState(30f, PropPhase.Holding, 0, 2, true)));
        }

        [Test]
        public void RestedPropIsInterrupted()
        {
            var windingUp = new PropState(-45f, PropPhase.WindingUp, 2, 7, false);

            Assert.That(_simulator.Rested(windingUp), Is.EqualTo(_simulator.Interrupted(windingUp)));
        }

        [Test]
        public void InterruptedPropNeedsAFreshPressToWindUp()
        {
            var interrupted = _simulator.Interrupted(Holding);

            var state = _simulator.Step(interrupted, Pressed());

            Assert.That(state.Phase, Is.EqualTo(PropPhase.Holding));
        }

        [Test]
        public void ShortWindUpLastsOneTick()
        {
            _config.WindUpTime = 0.01f;

            var state = _simulator.Step(_simulator.Step(Holding, Pressed()), Pressed());

            Assert.That(state.Phase, Is.EqualTo(PropPhase.Holding));
            Assert.That(state.Throws, Is.EqualTo(1));
        }

        [Test]
        public void UnknownPhaseThrows()
        {
            var broken = new PropState(0f, (PropPhase)7, 0, 0, false);

            Assert.Throws<ArgumentOutOfRangeException>(() => _simulator.Step(broken, Pressed()));
        }

        private static WeaponInput Pressed(float aim = 0f) =>
            new(aim, true, false, false);

        private static WeaponInput Released(float aim = 0f) =>
            new(aim, false, false, false);
    }
}
