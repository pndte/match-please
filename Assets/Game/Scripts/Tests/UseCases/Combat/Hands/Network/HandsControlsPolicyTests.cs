using Bw.Entities.Network.LagCompensation;
using Bw.Tests.Doubles;
using Bw.UseCases.Combat.Hands.Network;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Combat.Hands.Network
{
    public sealed class HandsControlsPolicyTests
    {
        private const int RewindTicks = 4;

        private WeaponInventoryConfig _config;
        private HandsControlsPolicy _policy;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<WeaponInventoryConfig>();
            var lagCompensation = new LagCompensationConfig { MaxRewindSeconds = 0.5f };
            _policy = new HandsControlsPolicy(lagCompensation, new ManualClock(0.125f), _config);
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [TestCase(DrawControlKind.Keep, 0)]
        [TestCase(DrawControlKind.Last, 0)]
        [TestCase(DrawControlKind.Slot, 0)]
        [TestCase(DrawControlKind.Slot, 2)]
        [TestCase(DrawControlKind.Scroll, 1)]
        [TestCase(DrawControlKind.Scroll, -1)]
        public void WellFormedChoiceIsValid(DrawControlKind kind, int value)
        {
            Assert.That(_policy.IsValid(Choosing(new DrawControl(kind, (sbyte)value))), Is.True);
        }

        [TestCase(DrawControlKind.Keep, 1)]
        [TestCase(DrawControlKind.Last, -1)]
        [TestCase(DrawControlKind.Slot, 3)]
        [TestCase(DrawControlKind.Slot, -1)]
        [TestCase(DrawControlKind.Scroll, 0)]
        [TestCase(DrawControlKind.Scroll, 2)]
        [TestCase((DrawControlKind)9, 0)]
        public void MalformedChoiceIsInvalid(DrawControlKind kind, int value)
        {
            Assert.That(_policy.IsValid(Choosing(new DrawControl(kind, (sbyte)value))), Is.False);
        }

        [TestCase(float.NaN, 0f)]
        [TestCase(0f, float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity, 1f)]
        public void BrokenAimPointIsInvalid(float x, float y)
        {
            Assert.That(_policy.IsValid(new HandsControls(new Vector2(x, y), default, false, DrawControl.Keep)), Is.False);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void BrokenTriggerDelayIsInvalid(int broken)
        {
            var trigger = broken switch
            {
                0 => new TriggerControls(true, float.NaN, 0f, default),
                1 => new TriggerControls(true, 0f, float.PositiveInfinity, default),
                2 => new TriggerControls(true, 0f, 0f, new TriggerPress(1, 0, float.NaN, 0f, 0)),
                _ => new TriggerControls(true, 0f, 0f, new TriggerPress(1, 0, 0f, float.NegativeInfinity, 0))
            };

            Assert.That(_policy.IsValid(new HandsControls(Vector2.right, trigger, false, DrawControl.Keep)), Is.False);
        }

        [Test]
        public void LateInputKeepsItsOwnAimReloadAndChoice()
        {
            var controls = new HandsControls(new Vector2(4f, 4f), new TriggerControls(false, 0f, 0f, Press(4, 0)), true, DrawControl.Last);

            var caughtUp = _policy.CaughtUp(controls, Controls(Press(4, 2)), Controls(Press(4, 1)), 2);

            Assert.That(caughtUp.AimPoint, Is.EqualTo(new Vector2(4f, 4f)));
            Assert.That(caughtUp.ReloadPressed, Is.True);
            Assert.That(caughtUp.Choice, Is.EqualTo(DrawControl.Last));
        }

        [Test]
        public void PressAlreadyKnownOnlyAgesByATick()
        {
            var controls = new HandsControls(Vector2.right, new TriggerControls(true, 1f, 1f, Press(4, 0)), false, DrawControl.Keep);

            var caughtUp = _policy.CaughtUp(controls, Controls(Press(4, 6)), Controls(Press(4, 1)), 2);

            Assert.That(caughtUp.Trigger.Pulled, Is.True);
            Assert.That(caughtUp.Trigger.Press.Number, Is.EqualTo(4));
            Assert.That(caughtUp.Trigger.Press.Age, Is.EqualTo(7));
        }

        [Test]
        public void PressFromTheFutureIsTreatedAsKnown()
        {
            var caughtUp = _policy.CaughtUp(Controls(Press(4, 0)), Controls(Press(4, 6)), Controls(Press(5, 1)), -3);

            Assert.That(caughtUp.Trigger.Pulled, Is.False);
            Assert.That(caughtUp.Trigger.Press.Number, Is.EqualTo(4));
            Assert.That(caughtUp.Trigger.Press.Age, Is.EqualTo(7));
        }

        [Test]
        public void NewPressHeardLateIsPulledNowWithItsDelaysGrownAndDatedBack()
        {
            var heard = Controls(new TriggerPress(5, 1, 2f, 1.5f, 0));

            var caughtUp = _policy.CaughtUp(Controls(Press(4, 0)), Controls(Press(4, 10)), heard, 2);

            Assert.That(caughtUp.Trigger.Pulled, Is.True);
            Assert.That(caughtUp.Trigger.ViewDelay, Is.EqualTo(5f));
            Assert.That(caughtUp.Trigger.PredictedDelay, Is.EqualTo(4.5f));
            Assert.That(caughtUp.Trigger.Press.Number, Is.EqualTo(5));
            Assert.That(caughtUp.Trigger.Press.Age, Is.Zero);
            Assert.That(caughtUp.Trigger.Press.Late, Is.EqualTo(3));
        }

        [Test]
        public void NewPressAtTheRewindLimitIsStillPulled()
        {
            var caughtUp = _policy.CaughtUp(Controls(Press(4, 0)), Controls(Press(4, 10)), Controls(Press(5, 1)), RewindTicks - 1);

            Assert.That(caughtUp.Trigger.Pulled, Is.True);
            Assert.That(caughtUp.Trigger.Press.Late, Is.EqualTo(RewindTicks));
        }

        [Test]
        public void NewPressOlderThanTheRewindIsOnlyRecordedWithItsAge()
        {
            var caughtUp = _policy.CaughtUp(Controls(Press(4, 0)), Controls(Press(4, 10)), Controls(Press(5, 1)), RewindTicks);

            Assert.That(caughtUp.Trigger.Pulled, Is.False);
            Assert.That(caughtUp.Trigger.Press.Number, Is.EqualTo(5));
            Assert.That(caughtUp.Trigger.Press.Age, Is.EqualTo(RewindTicks + 1));
            Assert.That(caughtUp.Trigger.Press.Late, Is.Zero);
        }

        private static HandsControls Choosing(DrawControl choice) =>
            new(Vector2.right, default, false, choice);

        private static HandsControls Controls(TriggerPress press) =>
            new(Vector2.right, new TriggerControls(false, 0f, 0f, press), false, DrawControl.Keep);

        private static TriggerPress Press(byte number, byte age) =>
            new(number, age, 1f, 1f, 0);
    }
}
