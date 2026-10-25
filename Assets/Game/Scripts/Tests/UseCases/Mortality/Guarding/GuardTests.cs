using System.Collections.Generic;
using Bw.Entities.Physics;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Guarding;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Bw.Tests.UseCases.Mortality.Guarding
{
    public sealed class GuardTests
    {
        private const float StepSeconds = 0.25f;
        private const int RegenDelayTicks = 4;
        private const int BrokenRegenDelayTicks = 8;
        private const int RegenIntervalTicks = 2;
        private const float RegenAmount = 10f;

        private static readonly Impacts Front = Blows.Of(Blows.At(0.5f, 0.3f, 30f));
        private static readonly Impacts Back = Blows.Of(Blows.At(-0.5f, 0.3f, 30f));

        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private GuardConfig _config;
        private BodyBox _box;
        private Guard _guard;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _config = ScriptableObject.CreateInstance<GuardConfig>();
            _config.MaxHealth = 100f;
            _config.Armor = 0f;
            _config.CoverAngle = 180f;
            _config.RegenDelay = RegenDelayTicks * StepSeconds;
            _config.BrokenRegenDelay = BrokenRegenDelayTicks * StepSeconds;
            _config.RegenInterval = RegenIntervalTicks * StepSeconds;
            _config.RegenAmount = RegenAmount;
            _box = _objects.Create("guarded").AddComponent<BodyBox>();
            _guard = new Guard(_config, new ManualClock(StepSeconds), _box);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void NewGuardIsWholeAndLowered()
        {
            Assert.That(_guard.Max, Is.EqualTo(100f));
            Assert.That(_guard.State.Value, Is.EqualTo(new GuardState(100f, false, false)));
            Assert.That(_guard.State.Value.Up, Is.False);
        }

        [Test]
        public void RaisedStanceRaisesTheGuard()
        {
            Hold(true, 0f);

            Assert.That(_guard.State.Value.Raised, Is.True);
            Assert.That(_guard.State.Value.Up, Is.True);
        }

        [Test]
        public void LoweredStanceLowersTheGuardAgain()
        {
            Hold(true, 0f);

            Hold(false, 0f);

            Assert.That(_guard.State.Value.Raised, Is.False);
            Assert.That(_guard.State.Value.Up, Is.False);
        }

        [Test]
        public void LoweredGuardCoversNothing()
        {
            Hold(false, 0f);

            Assert.That(_guard.Covers(Front[0].Contact), Is.False);
            Assert.That(_guard.Covers(Back[0].Contact), Is.False);
        }

        [TestCase(0f, 0.5f, 0.3f, true)]
        [TestCase(0f, -0.5f, 0.3f, false)]
        [TestCase(0f, 0f, 1f, true)]
        [TestCase(0f, -0.2f, 1f, false)]
        [TestCase(90f, 0f, 1f, true)]
        [TestCase(90f, 0.3f, -1f, false)]
        [TestCase(180f, -0.5f, 0.3f, true)]
        [TestCase(180f, 0.5f, 0.3f, false)]
        [TestCase(-90f, 0.3f, -1f, true)]
        [TestCase(270f, 0.3f, -1f, true)]
        [TestCase(270f, 0.3f, 1f, false)]
        public void RaisedGuardCoversTheHalfItFaces(
            float aim,
            float x,
            float y,
            bool covered)
        {
            Hold(true, aim);

            Assert.That(_guard.Covers(new Contact(new Vector2(x, y))), Is.EqualTo(covered));
        }

        [Test]
        public void CoverIsMeasuredFromTheCentreOfTheBox()
        {
            _config.CoverAngle = 90f;
            var raisedBox = _objects.Create("raised box").AddComponent<BodyBox>().With("_offset", new Vector2(0f, 1f));
            var offsetGuard = new Guard(_config, new ManualClock(StepSeconds), raisedBox);
            var contact = new Contact(new Vector2(1f, 1.2f));
            Hold(true, 0f);
            offsetGuard.Step(new GuardStance(true, 0f));

            Assert.That(offsetGuard.Covers(contact), Is.True);
            Assert.That(_guard.Covers(contact), Is.False);
        }

        [Test]
        public void FullCircleCoverCoversTheBackToo()
        {
            _config.CoverAngle = 360f;

            Hold(true, 0f);

            Assert.That(_guard.Covers(Back[0].Contact), Is.True);
            Assert.That(_guard.Covers(new Contact(new Vector2(-1f, 0f))), Is.True);
        }

        [Test]
        public void CoveredBlowCostsTheGuardItsDamage()
        {
            Hold(true, 0f);

            _guard.Absorb(Front);

            Assert.That(_guard.State.Value.Health, Is.EqualTo(70f));
        }

        [Test]
        public void BlowFromBehindLeavesTheGuard()
        {
            Hold(true, 0f);

            _guard.Absorb(Back);

            Assert.That(_guard.State.Value.Health, Is.EqualTo(100f));
        }

        [Test]
        public void LoweredGuardAbsorbsNothing()
        {
            Hold(false, 0f);

            _guard.Absorb(Front);

            Assert.That(_guard.State.Value.Health, Is.EqualTo(100f));
        }

        [TestCase(0f, 70f)]
        [TestCase(0.25f, 77.5f)]
        [TestCase(0.5f, 85f)]
        [TestCase(1f, 100f)]
        public void ArmorReducesWhatTheGuardTakes(float armor, float left)
        {
            _config.Armor = armor;
            Hold(true, 0f);

            _guard.Absorb(Front);

            Assert.That(_guard.State.Value.Health, Is.EqualTo(left));
        }

        [Test]
        public void SplitBuckshotCostsTheGuardOnlyThePelletsItCovers()
        {
            Hold(true, 0f);

            _guard.Absorb(Blows.Of(Blows.At(0.5f, 0.3f, 12f), Blows.At(-0.5f, 0.3f, 12f), Blows.At(0.4f, -0.2f, 12f)));

            Assert.That(_guard.State.Value.Health, Is.EqualTo(76f));
        }

        [Test]
        public void RegenerationWaitsTheRegenDelayAfterTheLastBlow()
        {
            Hold(true, 0f);
            _guard.Absorb(Front);

            Steps(RegenDelayTicks - 1);
            var beforeDelay = _guard.State.Value.Health;
            Steps(1);

            Assert.That(beforeDelay, Is.EqualTo(70f));
            Assert.That(_guard.State.Value.Health, Is.EqualTo(70f + RegenAmount));
        }

        [Test]
        public void RegenerationAddsItsAmountOncePerInterval()
        {
            Hold(true, 0f);
            _guard.Absorb(Front);
            Steps(RegenDelayTicks);
            var healths = new List<float>();

            for (var index = 0; index < 2 * RegenIntervalTicks; index++)
            {
                Steps(1);
                healths.Add(_guard.State.Value.Health);
            }

            Assert.That(healths, Is.EqualTo(new[] { 80f, 90f, 90f, 100f }));
        }

        [Test]
        public void RegenerationStopsAtTheMaximum()
        {
            Hold(true, 0f);
            _guard.Absorb(Blows.Of(Blows.At(1f, 0f, 4f)));

            Steps(RegenDelayTicks + 3 * RegenIntervalTicks);

            Assert.That(_guard.State.Value.Health, Is.EqualTo(100f));
        }

        [Test]
        public void WholeGuardNeverChanges()
        {
            Hold(true, 0f);
            var changes = 0;
            _guard.State.Advise(_lifetime.Lifetime, _ => changes++);

            Steps(3 * BrokenRegenDelayTicks);

            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void LoweredGuardRegeneratesToo()
        {
            Hold(true, 0f);
            _guard.Absorb(Front);

            for (var index = 0; index < RegenDelayTicks; index++)
                Hold(false, 0f);

            Assert.That(_guard.State.Value.Health, Is.EqualTo(70f + RegenAmount));
            Assert.That(_guard.State.Value.Raised, Is.False);
        }

        [Test]
        public void NewBlowRestartsTheRegenDelay()
        {
            Hold(true, 0f);
            _guard.Absorb(Front);
            Steps(RegenDelayTicks - 1);
            _guard.Absorb(Blows.Of(Blows.At(1f, 0f, 10f)));

            Steps(RegenDelayTicks - 1);
            var beforeDelay = _guard.State.Value.Health;
            Steps(1);

            Assert.That(beforeDelay, Is.EqualTo(60f));
            Assert.That(_guard.State.Value.Health, Is.EqualTo(60f + RegenAmount));
        }

        [Test]
        public void EmptyingBlowIsAbsorbedWhole()
        {
            Hold(true, 0f);

            _guard.Absorb(Blows.Of(Blows.At(1f, 0f, 250f)));

            Assert.That(_guard.State.Value.Health, Is.Zero);
        }

        [Test]
        public void EmptyGuardStillCoversUntilItsNextStep()
        {
            Hold(true, 0f);

            _guard.Absorb(Blows.Of(Blows.At(1f, 0f, 250f)));

            Assert.That(_guard.State.Value.Broken, Is.False);
            Assert.That(_guard.Covers(Front[0].Contact), Is.True);
        }

        [Test]
        public void EmptyGuardBreaksOnItsNextStepAndStaysRaisedButDown()
        {
            Hold(true, 0f);
            _guard.Absorb(Blows.Of(Blows.At(1f, 0f, 250f)));

            Steps(1);

            Assert.That(_guard.State.Value, Is.EqualTo(new GuardState(0f, true, true)));
            Assert.That(_guard.State.Value.Up, Is.False);
        }

        [Test]
        public void BrokenGuardCoversNothing()
        {
            Break();

            Assert.That(_guard.Covers(Front[0].Contact), Is.False);
        }

        [Test]
        public void BrokenGuardMendsAfterTheBrokenDelayWithOneRegen()
        {
            Break();

            Steps(BrokenRegenDelayTicks - 2);
            var beforeDelay = _guard.State.Value;
            Steps(1);

            Assert.That(beforeDelay.Broken, Is.True);
            Assert.That(_guard.State.Value, Is.EqualTo(new GuardState(RegenAmount, true, false)));
            Assert.That(_guard.State.Value.Up, Is.True);
        }

        [Test]
        public void MendedGuardRegeneratesAfterAnInterval()
        {
            Break();
            Steps(BrokenRegenDelayTicks - 1);

            Steps(RegenIntervalTicks - 1);
            var beforeInterval = _guard.State.Value.Health;
            Steps(1);

            Assert.That(beforeInterval, Is.EqualTo(RegenAmount));
            Assert.That(_guard.State.Value.Health, Is.EqualTo(2f * RegenAmount));
        }

        [Test]
        public void MendingNeverExceedsTheMaximum()
        {
            _config.RegenAmount = 150f;
            Break();

            Steps(BrokenRegenDelayTicks - 1);

            Assert.That(_guard.State.Value.Health, Is.EqualTo(100f));
            Assert.That(_guard.State.Value.Broken, Is.False);
        }

        [Test]
        public void BlowOnABrokenGuardDoesNotDelayTheMending()
        {
            Break();
            Steps(2);

            _guard.Absorb(Front);
            Steps(BrokenRegenDelayTicks - 4);
            var beforeDelay = _guard.State.Value.Broken;
            Steps(1);

            Assert.That(beforeDelay, Is.True);
            Assert.That(_guard.State.Value.Broken, Is.False);
        }

        [Test]
        public void LoweringABrokenGuardKeepsItBroken()
        {
            Break();

            Hold(false, 0f);

            Assert.That(_guard.State.Value, Is.EqualTo(new GuardState(0f, false, true)));
        }

        [Test]
        public void LoweredBrokenGuardMendsLoweredAndDown()
        {
            Break();

            for (var index = 1; index < BrokenRegenDelayTicks; index++)
                Hold(false, 0f);

            Assert.That(_guard.State.Value, Is.EqualTo(new GuardState(RegenAmount, false, false)));
            Assert.That(_guard.State.Value.Up, Is.False);
        }

        [Test]
        public void AppliedStateIsShownAsItCame()
        {
            var shown = new GuardState(42.5f, true, false);

            _guard.Apply(shown);

            Assert.That(_guard.State.Value, Is.EqualTo(shown));
            Assert.That(_guard.State.Value.Up, Is.True);
        }

        [Test]
        public void ShippedGuardRegeneratesTwoSecondsAfterABlowAtThirtyTicks()
        {
            var shipped = AssetDatabase.LoadAssetAtPath<GuardConfig>("Assets/Resources/GuardConfig.asset");
            var guard = new Guard(shipped, new ManualClock(1f / 30f), _box);
            var delayTicks = Mathf.CeilToInt(shipped.RegenDelay * 30f);
            guard.Step(new GuardStance(true, 0f));
            guard.Absorb(Front);

            for (var index = 1; index < delayTicks; index++)
                guard.Step(new GuardStance(true, 0f));

            var beforeDelay = guard.State.Value.Health;
            guard.Step(new GuardStance(true, 0f));

            Assert.That(delayTicks, Is.EqualTo(60));
            Assert.That(beforeDelay, Is.EqualTo(70f));
            Assert.That(guard.State.Value.Health, Is.EqualTo(70f + shipped.RegenAmount));
        }

        private void Hold(bool raised, float aim) =>
            _guard.Step(new GuardStance(raised, aim));

        private void Steps(int count)
        {
            for (var index = 0; index < count; index++)
                Hold(true, 0f);
        }

        private void Break()
        {
            Hold(true, 0f);
            _guard.Absorb(Blows.Of(Blows.At(1f, 0f, 250f)));
            Steps(1);
        }
    }
}
