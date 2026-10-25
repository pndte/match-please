using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Abstractions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement
{
    public sealed class BodyStateTests
    {
        private static readonly BodyState State = new(new Vector2(1f, 2f), new Vector2(3f, 4f), 5f, true);

        [Test]
        public void StatesWithTheSameFieldsAreEqual()
        {
            Assert.That(State.Equals(new BodyState(new Vector2(1f, 2f), new Vector2(3f, 4f), 5f, true)), Is.True);
        }

        [Test]
        public void AnyFieldMakesADifference()
        {
            Assert.That(State.Equals(new BodyState(new Vector2(1.01f, 2f), new Vector2(3f, 4f), 5f, true)), Is.False);
            Assert.That(State.Equals(new BodyState(new Vector2(1f, 2f), new Vector2(3f, 4.01f), 5f, true)), Is.False);
            Assert.That(State.Equals(new BodyState(new Vector2(1f, 2f), new Vector2(3f, 4f), 5.01f, true)), Is.False);
            Assert.That(State.Equals(new BodyState(new Vector2(1f, 2f), new Vector2(3f, 4f), 5f, false)), Is.False);
        }

        [Test]
        public void PlacedAtItsPositionUpright()
        {
            Assert.That(PositionOf(State), Is.EqualTo(new Vector2(1f, 2f)));
            Assert.That(AngleOf(State), Is.EqualTo(0f));
        }

        [Test]
        public void ShiftMovesOnlyThePosition()
        {
            Assert.That(Shifted(State, new Vector2(10f, -1f)), Is.EqualTo(new BodyState(new Vector2(11f, 1f), new Vector2(3f, 4f), 5f, true)));
        }

        [Test]
        public void BodyOfABodyStateIsTheStateItself()
        {
            var other = new BodyState(Vector2.zero, Vector2.one, 0f, false);

            Assert.That(BodyOf(State), Is.EqualTo(State));
            Assert.That(WithBody(State, other), Is.EqualTo(other));
        }

        private static Vector2 PositionOf<TState>(TState state) where TState : struct, IPlacedState<TState> =>
            state.Position;

        private static float AngleOf<TState>(TState state) where TState : struct, IPlacedState<TState> =>
            state.Angle;

        private static TState Shifted<TState>(TState state, Vector2 offset) where TState : struct, IPlacedState<TState> =>
            state.Shifted(offset);

        private static BodyState BodyOf<TState>(TState state) where TState : struct, IBodyState<TState> =>
            state.Body;

        private static TState WithBody<TState>(TState state, BodyState body) where TState : struct, IBodyState<TState> =>
            state.WithBody(body);
    }
}
