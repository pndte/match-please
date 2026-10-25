using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Abstractions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement
{
    public sealed class RigidStateTests
    {
        private static readonly RigidState State = new(new Vector2(1f, 2f), 30f, new Vector2(3f, 4f), 90f);

        [Test]
        public void StatesWithTheSameFieldsAreEqual()
        {
            Assert.That(State.Equals(new RigidState(new Vector2(1f, 2f), 30f, new Vector2(3f, 4f), 90f)), Is.True);
        }

        [Test]
        public void AnyFieldMakesADifference()
        {
            Assert.That(State.Equals(new RigidState(new Vector2(1f, 2.01f), 30f, new Vector2(3f, 4f), 90f)), Is.False);
            Assert.That(State.Equals(new RigidState(new Vector2(1f, 2f), 30.01f, new Vector2(3f, 4f), 90f)), Is.False);
            Assert.That(State.Equals(new RigidState(new Vector2(1f, 2f), 30f, new Vector2(3.01f, 4f), 90f)), Is.False);
            Assert.That(State.Equals(new RigidState(new Vector2(1f, 2f), 30f, new Vector2(3f, 4f), 90.01f)), Is.False);
        }

        [Test]
        public void PlacedAtItsPositionAndAngle()
        {
            Assert.That(PositionOf(State), Is.EqualTo(new Vector2(1f, 2f)));
            Assert.That(AngleOf(State), Is.EqualTo(30f));
        }

        [Test]
        public void ShiftMovesOnlyThePosition()
        {
            Assert.That(Shifted(State, new Vector2(1f, 1f)), Is.EqualTo(new RigidState(new Vector2(2f, 3f), 30f, new Vector2(3f, 4f), 90f)));
        }

        private static Vector2 PositionOf<TState>(TState state) where TState : struct, IPlacedState<TState> =>
            state.Position;

        private static float AngleOf<TState>(TState state) where TState : struct, IPlacedState<TState> =>
            state.Angle;

        private static TState Shifted<TState>(TState state, Vector2 offset) where TState : struct, IPlacedState<TState> =>
            state.Shifted(offset);
    }
}
