using Bw.Entities.Network.Rollback;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.UseCases.Movement.Network.Prediction;
using JetBrains.Collections.Viewable;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Movement.Network.Prediction
{
    public sealed class PredictedBodyTypeTests
    {
        private PhysicsTestWorld _world;
        private PhysicsBody _body;
        private Predicted _predicted;

        [SetUp]
        public void SetUp()
        {
            _world = new PhysicsTestWorld();
            _body = _world.Dynamic("Crate", new Vector2(0f, 5f), Vector2.one);
            _predicted = new Predicted();
            new PredictedBodyType(_world.Lifetime, _predicted, _body);
        }

        [TearDown]
        public void TearDown() =>
            _world.Dispose();

        [Test]
        public void BodyNotPredictedIsKinematic()
        {
            Assert.That(_body.type, Is.EqualTo(PhysicsBody.BodyType.Kinematic));
        }

        [Test]
        public void PredictedBodyIsDynamic()
        {
            _predicted.Me.Value = true;

            Assert.That(_body.type, Is.EqualTo(PhysicsBody.BodyType.Dynamic));
        }

        [Test]
        public void PredictedBodyFallsUnderGravity()
        {
            _predicted.Me.Value = true;

            _world.Step(10);

            Assert.That(_body.position.y, Is.LessThan(5f));
        }

        [Test]
        public void BodyLeavingPredictionStopsWhereItIs()
        {
            _predicted.Me.Value = true;
            _world.Step(10);
            var left = _body.position;

            _predicted.Me.Value = false;
            _world.Step(10);

            Assert.That(_body.type, Is.EqualTo(PhysicsBody.BodyType.Kinematic));
            Assert.That(_body.linearVelocity, Is.EqualTo(Vector2.zero));
            Assert.That(_body.angularVelocity, Is.Zero);
            Assert.That(_body.position, Is.EqualTo(left));
        }

        private sealed class Predicted : IPredictedBy
        {
            public ViewableProperty<bool> Me { get; } = new(false);

            IReadonlyProperty<bool> IPredictedBy.Me => Me;
        }
    }
}
