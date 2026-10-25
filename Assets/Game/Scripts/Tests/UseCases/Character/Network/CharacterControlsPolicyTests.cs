using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Tests.UseCases.Character.Doubles;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Movement.Network.Requests;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Character.Network
{
    public sealed class CharacterControlsPolicyTests
    {
        private static readonly MovementControls CaughtUpMovement = new(-1f, true, true, true);
        private static readonly HandsControls CaughtUpHands = new(90f, default, true, DrawControl.Last);

        private RecordedPolicy<MovementControls> _movement;
        private RecordedPolicy<HandsControls> _hands;
        private CharacterControlsPolicy _policy;

        [SetUp]
        public void SetUp()
        {
            _movement = new RecordedPolicy<MovementControls>(CaughtUpMovement);
            _hands = new RecordedPolicy<HandsControls>(CaughtUpHands);
            _policy = new CharacterControlsPolicy(_movement, _hands);
        }

        [Test]
        public void ControlsValidInEveryPartAreValid()
        {
            Assert.That(_policy.IsValid(Controls.RunningAndFiring), Is.True);
        }

        [Test]
        public void InvalidMovementMakesTheControlsInvalid()
        {
            _movement.Valid = false;

            Assert.That(_policy.IsValid(Controls.RunningAndFiring), Is.False);
        }

        [Test]
        public void InvalidHandsMakeTheControlsInvalid()
        {
            _hands.Valid = false;

            Assert.That(_policy.IsValid(Controls.RunningAndFiring), Is.False);
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void GuardAimThatIsNotFiniteIsInvalid(float aim)
        {
            var controls = new CharacterControls(Controls.Running, Controls.Firing, new GuardStance(true, aim));

            Assert.That(_policy.IsValid(controls), Is.False);
        }

        [Test]
        public void ValidityAsksEachPartAboutItsOwnControls()
        {
            _policy.IsValid(Controls.RunningAndFiring);

            Assert.That(_movement.Checked, Is.EqualTo(new[] { Controls.Running }));
            Assert.That(_hands.Checked, Is.EqualTo(new[] { Controls.Firing }));
        }

        [Test]
        public void CaughtUpControlsAreTheCaughtUpParts()
        {
            var controls = Controls.RunningAndFiring;

            var caught = _policy.CaughtUp(controls, controls, controls, 3);

            Assert.That(caught.Movement, Is.EqualTo(CaughtUpMovement));
            Assert.That(caught.Hands, Is.EqualTo(CaughtUpHands));
        }

        [Test]
        public void CatchingUpHandsEachPartItsOwnInputsAndTheAge()
        {
            var controls = new CharacterControls(new MovementControls(0.25f, false, false, false), Hands(10f), GuardStance.Lowered);
            var last = new CharacterControls(new MovementControls(0.5f, false, false, false), Hands(20f), GuardStance.Lowered);
            var heard = new CharacterControls(new MovementControls(0.75f, false, false, false), Hands(30f), GuardStance.Lowered);

            _policy.CaughtUp(controls, last, heard, 3);

            Assert.That(_movement.CaughtUpFrom, Is.EqualTo(new[] { (controls.Movement, last.Movement, heard.Movement, 3) }));
            Assert.That(_hands.CaughtUpFrom, Is.EqualTo(new[] { (controls.Hands, last.Hands, heard.Hands, 3) }));
        }

        [Test]
        public void CatchingUpKeepsTheGuardOfTheControls()
        {
            var controls = new CharacterControls(Controls.Running, Controls.Firing, new GuardStance(true, 60f));
            var last = new CharacterControls(Controls.Running, Controls.Firing, new GuardStance(false, 0f));
            var heard = new CharacterControls(Controls.Running, Controls.Firing, new GuardStance(true, -30f));

            var guard = _policy.CaughtUp(controls, last, heard, 1).Guard;

            Assert.That(guard.Raised, Is.True);
            Assert.That(guard.Aim, Is.EqualTo(60f));
        }

        private static HandsControls Hands(float aim) =>
            new(aim, default, false, DrawControl.Keep);

        private sealed class RecordedPolicy<TInput> : IInputPolicy<TInput> where TInput : struct
        {
            public bool Valid { get; set; } = true;
            public List<TInput> Checked { get; } = new();
            public List<(TInput Input, TInput Last, TInput Heard, int Ago)> CaughtUpFrom { get; } = new();

            private readonly TInput _caughtUp;

            public RecordedPolicy(TInput caughtUp)
            {
                _caughtUp = caughtUp;
            }

            public bool IsValid(TInput input)
            {
                Checked.Add(input);
                return Valid;
            }

            public TInput CaughtUp(
                TInput input,
                TInput lastInput,
                TInput heard,
                int heardTicksAgo)
            {
                CaughtUpFrom.Add((input, lastInput, heard, heardTicksAgo));
                return _caughtUp;
            }
        }
    }
}
