using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement.Extensions
{
    public sealed class BodyStateExtensionsTests
    {
        private static readonly BodyState Body = new(new Vector2(1f, 2f), new Vector2(3f, -4f), 0.5f, true);
        private static readonly JumpState Jump = new(false, 1, 2);

        [Test]
        public void PushAddsItsVerticalPartToTheVelocityAndItsHorizontalPartToThePush()
        {
            Assert.That(Body.Pushed(new Vector2(2f, 6f)), Is.EqualTo(new BodyState(new Vector2(1f, 2f), new Vector2(3f, 2f), 2.5f, true)));
        }

        [Test]
        public void ShiftMovesOnlyThePosition()
        {
            Assert.That(Body.Shifted(new Vector2(-1f, 0.5f)), Is.EqualTo(new BodyState(new Vector2(0f, 2.5f), new Vector2(3f, -4f), 0.5f, true)));
        }

        [Test]
        public void DriftAddsToTheVelocityOnly()
        {
            Assert.That(Body.Drifted(new Vector2(1f, 1f)), Is.EqualTo(new BodyState(new Vector2(1f, 2f), new Vector2(4f, -3f), 0.5f, true)));
        }

        [Test]
        public void PushOfAWholeStateChangesItsBodyAndKeepsTheRest()
        {
            var pushed = new MovementState(Body, Jump).Pushed(new Vector2(2f, 6f));

            Assert.That(pushed.Body, Is.EqualTo(Body.Pushed(new Vector2(2f, 6f))));
            Assert.That(pushed.Jump, Is.EqualTo(Jump));
        }

        [Test]
        public void DriftOfAWholeStateChangesItsBodyAndKeepsTheRest()
        {
            var drifted = new MovementState(Body, Jump).Drifted(new Vector2(1f, 1f));

            Assert.That(drifted.Body, Is.EqualTo(Body.Drifted(new Vector2(1f, 1f))));
            Assert.That(drifted.Jump, Is.EqualTo(Jump));
        }
    }
}
