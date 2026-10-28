using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Bubble;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.Entities.Network.Prediction.Bubble
{
    public sealed class BubbleMembershipTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private ScriptedWorldStep _worldStep;
        private ScriptedBubble _bubble;
        private Transform _transform;
        private BubbleMembership _membership;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _worldStep = new ScriptedWorldStep();
            _bubble = new ScriptedBubble();
            _transform = _objects.Create("member", new Vector2(3f, 4f)).transform;
            _membership = new BubbleMembership(_lifetime.Lifetime, _transform, _bubble, _worldStep);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void MemberStartsOutside()
        {
            Assert.That(_membership.Inside.Value, Is.False);
            Assert.That(_bubble.Asked, Is.Empty);
        }

        [Test]
        public void PrepareAsksTheBubbleAboutThePositionAndTheCurrentMembership()
        {
            _bubble.Answer = true;

            _worldStep.Fire(TickPhase.Prepare, 1);
            _transform.position = new Vector2(-1f, 2f);
            _worldStep.Fire(TickPhase.Prepare, 2);

            Assert.That(_bubble.Asked, Is.EqualTo(new[] { (new Vector2(3f, 4f), false), (new Vector2(-1f, 2f), true) }));
        }

        [Test]
        public void MembershipFollowsTheBubbleAnswer()
        {
            _bubble.Answer = true;
            _worldStep.Fire(TickPhase.Prepare, 1);
            var entered = _membership.Inside.Value;
            _bubble.Answer = false;
            _worldStep.Fire(TickPhase.Prepare, 2);

            Assert.That(entered, Is.True);
            Assert.That(_membership.Inside.Value, Is.False);
        }

        [Test]
        public void PredictionIsTheMembership()
        {
            var predictedBy = (IPredictedBy)_membership;
            _bubble.Answer = true;

            _worldStep.Fire(TickPhase.Prepare, 1);

            Assert.That(predictedBy.Me.Value, Is.True);
            Assert.That(predictedBy.Me, Is.SameAs(_membership.Inside));
        }

        [TestCase(TickPhase.BeforePhysics)]
        [TestCase(TickPhase.Default)]
        [TestCase(TickPhase.Late)]
        [TestCase(TickPhase.Publish)]
        public void OnlyPrepareSettlesTheMembership(TickPhase phase)
        {
            _bubble.Answer = true;

            _worldStep.Fire(phase, 1);

            Assert.That(_membership.Inside.Value, Is.False);
            Assert.That(_bubble.Asked, Is.Empty);
        }

        [Test]
        public void EndedMembershipStopsAsking()
        {
            _bubble.Answer = true;
            _lifetime.Terminate();

            _worldStep.Fire(TickPhase.Prepare, 1);

            Assert.That(_bubble.Asked, Is.Empty);
            Assert.That(_membership.Inside.Value, Is.False);
        }

        private sealed class ScriptedBubble : IPredictionBubble
        {
            public bool Answer { get; set; }
            public List<(Vector2 Position, bool Inside)> Asked { get; } = new();

            public bool Contains(Vector2 position, bool inside)
            {
                Asked.Add((position, inside));
                return Answer;
            }
        }
    }
}
