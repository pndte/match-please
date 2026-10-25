using Bw.Tests.UseCases.Movement.Doubles;
using Bw.UseCases.Movement;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement
{
    public sealed class BodyStateViewTests
    {
        [TestCase(0f, 0f, 0f)]
        [TestCase(0.25f, 1f, 0.5f)]
        [TestCase(1f, 4f, 2f)]
        public void BodyIsPlacedBetweenTheStatesByTheProgress(float progress, float x, float y)
        {
            var body = new PlacedBody();
            var view = new BodyStateView<MovementState>(body);

            view.Show(At(Vector2.zero), At(new Vector2(4f, 2f)), progress);

            Assert.That(body.Placed, Is.EqualTo(new[] { new Vector2(x, y) }));
        }

        private static MovementState At(Vector2 position) =>
            new(new BodyState(position, Vector2.zero, 0f, true), default);
    }
}
