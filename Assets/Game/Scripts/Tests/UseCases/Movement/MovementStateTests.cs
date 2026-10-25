using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Abstractions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement
{
    public sealed class MovementStateTests
    {
        private static readonly BodyState Body = new(new Vector2(1f, 2f), new Vector2(3f, 4f), 5f, true);
        private static readonly JumpState Jump = new(true, 2, 1);
        private static readonly MovementState State = new(Body, Jump);

        [Test]
        public void StatesWithTheSameBodyAndJumpAreEqual()
        {
            Assert.That(State.Equals(new MovementState(Body, new JumpState(true, 2, 1))), Is.True);
        }

        [Test]
        public void BodyOrJumpMakesADifference()
        {
            Assert.That(State.Equals(new MovementState(new BodyState(Vector2.zero, new Vector2(3f, 4f), 5f, true), Jump)), Is.False);
            Assert.That(State.Equals(new MovementState(Body, new JumpState(true, 2, 0))), Is.False);
        }

        [Test]
        public void PlacedAtTheBodysPositionUpright()
        {
            Assert.That(PositionOf(State), Is.EqualTo(new Vector2(1f, 2f)));
            Assert.That(AngleOf(State), Is.EqualTo(0f));
        }

        [Test]
        public void ShiftMovesTheBodyAndKeepsTheJump()
        {
            var shifted = Shifted(State, new Vector2(-1f, 1f));

            Assert.That(shifted.Body, Is.EqualTo(new BodyState(new Vector2(0f, 3f), new Vector2(3f, 4f), 5f, true)));
            Assert.That(shifted.Jump, Is.EqualTo(Jump));
        }

        [Test]
        public void WithBodyReplacesTheBodyAndKeepsTheJump()
        {
            var body = new BodyState(Vector2.one, Vector2.zero, 0f, false);

            var replaced = WithBody(State, body);

            Assert.That(replaced.Body, Is.EqualTo(body));
            Assert.That(replaced.Jump, Is.EqualTo(Jump));
        }

        private static Vector2 PositionOf<TState>(TState state) where TState : struct, IPlacedState<TState> =>
            state.Position;

        private static float AngleOf<TState>(TState state) where TState : struct, IPlacedState<TState> =>
            state.Angle;

        private static TState Shifted<TState>(TState state, Vector2 offset) where TState : struct, IPlacedState<TState> =>
            state.Shifted(offset);

        private static TState WithBody<TState>(TState state, BodyState body) where TState : struct, IBodyState<TState> =>
            state.WithBody(body);
    }
}
