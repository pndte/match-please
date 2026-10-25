using Bw.Tests.Doubles;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Hands.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Hands
{
    public sealed class HoldOvalExtensionsTests
    {
        private HiddenObjects _objects;

        [SetUp]
        public void SetUp() =>
            _objects = new HiddenObjects();

        [TearDown]
        public void TearDown() =>
            _objects.Dispose();

        [Test]
        public void PoseIsTheHoldersPositionAndTheOvalsBoundsAroundIt()
        {
            var oval = _objects.Create("holder").AddComponent<HoldOval>()
                .With("_center", new Vector2(0.25f, 0.5f))
                .With("_radii", new Vector2(0.5f, 1f));

            var pose = oval.PoseAt(new Vector2(3f, 4f));

            Assert.That(pose.Position, Is.EqualTo(new Vector2(3f, 4f)));
            Assert.That(pose.Hold, Is.EqualTo(new Rect(2.75f, 3.5f, 1f, 2f)));
        }

        [Test]
        public void PoseFollowsThePositionAskedForNotTheOvalsTransform()
        {
            var oval = _objects.Create("holder", new Vector2(-10f, -10f)).AddComponent<HoldOval>()
                .With("_radii", new Vector2(1f, 1f));

            var pose = oval.PoseAt(Vector2.zero);

            Assert.That(pose.Hold.center, Is.EqualTo(Vector2.zero));
        }
    }
}
