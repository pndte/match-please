using System.Collections.Generic;
using Bw.Tests.UseCases.Movement.Doubles;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement
{
    public sealed class CharacterMovementTests
    {
        private LifetimeDefinition _lifetime;
        private ShiftingMotor _motor;
        private CharacterMovement _movement;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _motor = new ShiftingMotor();
            _movement = new CharacterMovement(_motor, new PlacedBody(new Vector2(3f, 4f)));
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void StartsAtTheBodyAtRestAirborneAndNotJumping()
        {
            var expected = new MovementState(new BodyState(new Vector2(3f, 4f), Vector2.zero, 0f, false), new JumpState(false, 0, 0));

            Assert.That(_movement.State.Value, Is.EqualTo(expected));
            Assert.That(_movement.Previous, Is.EqualTo(expected));
        }

        [Test]
        public void StepRunsTheMotorFromTheCurrentStateWithTheInput()
        {
            var start = _movement.State.Value;

            _movement.Step(new MovementInput(1f, true, false, false, default));

            Assert.That(_motor.Steps, Has.Count.EqualTo(1));
            Assert.That(_motor.Steps[0].State, Is.EqualTo(start));
            Assert.That(_motor.Steps[0].Input.JumpPressed, Is.True);
            Assert.That(_movement.State.Value.Body.Position, Is.EqualTo(new Vector2(4f, 4f)));
            Assert.That(_movement.Previous, Is.EqualTo(start));
        }

        [Test]
        public void StepAnnouncesTheNewState()
        {
            var stepped = new List<MovementState>();
            _movement.Stepped.Advise(_lifetime.Lifetime, stepped.Add);

            _movement.Step(default);
            _movement.Step(default);

            Assert.That(stepped, Has.Count.EqualTo(2));
            Assert.That(stepped[1], Is.EqualTo(_movement.State.Value));
            Assert.That(stepped[1].Body.Position, Is.EqualTo(new Vector2(5f, 4f)));
        }

        [Test]
        public void ApplyRestoresTheStateWithoutAStep()
        {
            var stepped = 0;
            _movement.Stepped.Advise(_lifetime.Lifetime, _ => stepped++);
            var previous = _movement.Previous;
            var restored = new MovementState(new BodyState(new Vector2(-1f, 0f), Vector2.one, 0f, true), new JumpState(true, 1, 2));

            _movement.Apply(restored);

            Assert.That(_movement.State.Value, Is.EqualTo(restored));
            Assert.That(_movement.Previous, Is.EqualTo(previous));
            Assert.That(stepped, Is.EqualTo(0));
            Assert.That(_motor.Steps, Is.Empty);
        }

        [Test]
        public void StepAfterApplyStartsFromTheAppliedState()
        {
            var restored = new MovementState(new BodyState(new Vector2(-1f, 0f), Vector2.one, 0f, true), new JumpState(true, 1, 2));
            _movement.Apply(restored);

            _movement.Step(default);

            Assert.That(_movement.Previous, Is.EqualTo(restored));
            Assert.That(_movement.State.Value.Body.Position, Is.EqualTo(new Vector2(0f, 0f)));
        }

        private sealed class ShiftingMotor : IMovementMotor
        {
            public List<(MovementState State, MovementInput Input)> Steps { get; } = new();

            public MovementState Step(MovementState state, MovementInput input)
            {
                Steps.Add((state, input));
                return new MovementState(state.Body.Shifted(Vector2.right), state.Jump);
            }
        }
    }
}
