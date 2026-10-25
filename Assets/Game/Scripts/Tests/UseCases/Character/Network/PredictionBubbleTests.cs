using Bw.Entities.Network.Prediction.Bubble;
using Bw.Tests.UseCases.Character.Doubles;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Combat.Characters;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Character.Network
{
    public sealed class PredictionBubbleTests
    {
        private LifetimeDefinition _lifetime;
        private ControlledCharacters _centers;
        private PredictionBubble _bubble;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _centers = new ControlledCharacters();
            _bubble = new PredictionBubble(_centers, new PredictionBubbleConfig { EnterRadius = 10f, LeaveRadius = 15f });
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void BubbleWithoutCentersContainsNothing()
        {
            Assert.That(_bubble.Contains(Vector2.zero, false), Is.False);
            Assert.That(_bubble.Contains(Vector2.zero, true), Is.False);
        }

        [TestCase(9.5f, true)]
        [TestCase(10f, true)]
        [TestCase(10.5f, false)]
        public void ObjectOutsideEntersWithinTheEnterRadius(float distance, bool contained)
        {
            _centers.Add(_lifetime.Lifetime, new FakeCharacter(new Vector2(3f, 4f)));

            Assert.That(_bubble.Contains(new Vector2(3f, 4f + distance), false), Is.EqualTo(contained));
        }

        [TestCase(12f, true)]
        [TestCase(15f, true)]
        [TestCase(15.5f, false)]
        public void ObjectInsideStaysWithinTheLeaveRadius(float distance, bool contained)
        {
            _centers.Add(_lifetime.Lifetime, new FakeCharacter(new Vector2(3f, 4f)));

            Assert.That(_bubble.Contains(new Vector2(3f - distance, 4f), true), Is.EqualTo(contained));
        }

        [Test]
        public void AnyCenterContains()
        {
            _centers.Add(_lifetime.Lifetime, new FakeCharacter(new Vector2(-50f, 0f)));
            _centers.Add(_lifetime.Lifetime, new FakeCharacter(new Vector2(50f, 0f)));

            Assert.That(_bubble.Contains(new Vector2(45f, 0f), false), Is.True);
            Assert.That(_bubble.Contains(new Vector2(-45f, 0f), false), Is.True);
            Assert.That(_bubble.Contains(Vector2.zero, true), Is.False);
        }

        [Test]
        public void BubbleFollowsWhereTheCenterIsNow()
        {
            var center = new FakeCharacter(Vector2.zero);
            _centers.Add(_lifetime.Lifetime, center);

            center.MoveTo(new Vector2(100f, 0f));

            Assert.That(_bubble.Contains(Vector2.zero, true), Is.False);
            Assert.That(_bubble.Contains(new Vector2(95f, 0f), false), Is.True);
        }

        [Test]
        public void CenterThatLeftContainsNothing()
        {
            var controlled = _lifetime.Lifetime.CreateNested();
            _centers.Add(controlled.Lifetime, new FakeCharacter(Vector2.zero));

            controlled.Terminate();

            Assert.That(_bubble.Contains(Vector2.zero, true), Is.False);
        }
    }
}
