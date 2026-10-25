using Bw.UseCases.Props.View;
using Bw.UseCases.Props.View.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Props.View
{
    public sealed class ThrowArcTests
    {
        [Test]
        public void ArcsWithTheSameFieldsAreEqual()
        {
            var arc = new ThrowArc(Vector2.one, Vector2.right, Vector2.down, 0.5f);

            Assert.That(arc.Equals(new ThrowArc(Vector2.one, Vector2.right, Vector2.down, 0.5f)), Is.True);
        }

        [Test]
        public void EveryFieldTakesPartInEquality()
        {
            var arc = new ThrowArc(Vector2.one, Vector2.right, Vector2.down, 0.5f);

            Assert.That(arc.Equals(new ThrowArc(Vector2.zero, Vector2.right, Vector2.down, 0.5f)), Is.False);
            Assert.That(arc.Equals(new ThrowArc(Vector2.one, Vector2.left, Vector2.down, 0.5f)), Is.False);
            Assert.That(arc.Equals(new ThrowArc(Vector2.one, Vector2.right, Vector2.up, 0.5f)), Is.False);
            Assert.That(arc.Equals(new ThrowArc(Vector2.one, Vector2.right, Vector2.down, 0.6f)), Is.False);
        }

        [Test]
        public void ArcStartsAtItsOrigin()
        {
            var arc = new ThrowArc(new Vector2(2f, 3f), new Vector2(10f, 5f), new Vector2(0f, -20f), 1f);

            Assert.That(arc.PointAt(0f), Is.EqualTo(new Vector2(2f, 3f)));
        }

        [TestCase(0.5f, 7f, 3.0f)]
        [TestCase(1f, 12f, -2f)]
        [TestCase(0.25f, 4.5f, 3.625f)]
        public void PointFollowsTheBallisticFormula(float time, float x, float y)
        {
            var arc = new ThrowArc(new Vector2(2f, 3f), new Vector2(10f, 5f), new Vector2(0f, -20f), 1f);

            var point = arc.PointAt(time);

            Assert.That(point.x, Is.EqualTo(x).Within(1e-4f));
            Assert.That(point.y, Is.EqualTo(y).Within(1e-4f));
        }
    }
}
