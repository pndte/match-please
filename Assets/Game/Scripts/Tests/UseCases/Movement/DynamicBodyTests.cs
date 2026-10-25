using System;
using System.Collections.Generic;
using Bw.Entities.Physics.Extensions;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.UseCases.Level;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Movement
{
    public sealed class DynamicBodyTests
    {
        private PhysicsTestWorld _physics;
        private PhysicsBody _body;
        private DynamicBody _crate;

        [SetUp]
        public void SetUp()
        {
            _physics = new PhysicsTestWorld();
            _body = _physics.Dynamic("Crate", new Vector2(0f, 5f), Vector2.one);
            _crate = new DynamicBody(_physics.Lifetime, _body, _physics.Space);
        }

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [Test]
        public void StartsAtTheBodysState()
        {
            Assert.That(_crate.State.Value, Is.EqualTo(_body.State()));
            Assert.That(_crate.Previous, Is.EqualTo(_body.State()));
        }

        [Test]
        public void PhysicsStepIsReadIntoTheState()
        {
            var before = _crate.State.Value;
            var stepped = new List<RigidState>();
            _crate.Stepped.Advise(_physics.Lifetime, stepped.Add);

            _physics.Step();

            Assert.That(_crate.Previous, Is.EqualTo(before));
            Assert.That(_crate.State.Value, Is.EqualTo(_body.State()));
            Assert.That(_crate.State.Value.Velocity.y, Is.LessThan(0f));
            Assert.That(stepped, Is.EqualTo(new[] { _crate.State.Value }));
        }

        [Test]
        public void ApplyPutsTheBodyInTheStateWithoutAStep()
        {
            var stepped = 0;
            _crate.Stepped.Advise(_physics.Lifetime, _ => stepped++);
            var previous = _crate.Previous;
            var applied = new RigidState(new Vector2(3f, 1f), 20f, new Vector2(1f, 2f), 45f);

            _crate.Apply(applied);

            Assert.That(_crate.State.Value, Is.EqualTo(applied));
            Assert.That(_body.position, Is.EqualTo(new Vector2(3f, 1f)));
            Assert.That(_body.linearVelocity, Is.EqualTo(new Vector2(1f, 2f)));
            Assert.That(_crate.Previous, Is.EqualTo(previous));
            Assert.That(stepped, Is.EqualTo(0));
        }

        [Test]
        public void RestartTakesTheBodyBackIntoTheWorldAndStartsAnew()
        {
            var stepped = new List<RigidState>();
            _crate.Stepped.Advise(_physics.Lifetime, stepped.Add);
            _body.enabled = false;
            var restarted = new RigidState(new Vector2(-2f, 3f), 0f, new Vector2(4f, 0f), 0f);

            _crate.Restart(restarted);

            Assert.That(_body.enabled, Is.True);
            Assert.That(_body.position, Is.EqualTo(new Vector2(-2f, 3f)));
            Assert.That(_body.linearVelocity, Is.EqualTo(new Vector2(4f, 0f)));
            Assert.That(_crate.State.Value, Is.EqualTo(restarted));
            Assert.That(_crate.Previous, Is.EqualTo(restarted));
            Assert.That(stepped, Is.EqualTo(new[] { restarted }));
        }

        [Test]
        public void RestartedVelocitySurvivesTheNextStep()
        {
            _body.enabled = false;
            _crate.Restart(new RigidState(new Vector2(-2f, 3f), 0f, new Vector2(4f, 0f), 0f));

            _physics.Step();

            Assert.That(_crate.State.Value.Velocity.x, Is.EqualTo(4f).Within(1e-4f));
        }

        [TestCase(1f, 0f, 3f)]
        [TestCase(-1f, 0f, 0f)]
        [TestCase(0f, 1f, 0f)]
        [TestCase(0f, -1f, 4f)]
        public void RetreatSpeedIsTheVelocityAlongTheShoveNeverBelowZero(float x, float y, float speed)
        {
            _body.linearVelocity = new Vector2(3f, -4f);

            Assert.That(_crate.SpeedAway(new Vector2(x, y)), Is.EqualTo(speed).Within(1e-5f));
        }

        [Test]
        public void ShovePushesTheBodyInTheNextStep()
        {
            _body.gravityScale = 0f;

            _crate.Shove(new Vector2(30f, 0f));
            _physics.Step();

            Assert.That(_crate.State.Value.Velocity.x, Is.EqualTo(30f / _body.mass * _physics.Clock.Duration).Within(1e-3f));
        }

        [TestCase(float.NaN, 0f)]
        [TestCase(0f, float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity, 1f)]
        public void ShoveWithAnInfiniteForceThrows(float x, float y)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _crate.Shove(new Vector2(x, y)));
        }

        [Test]
        public void LandingOnASpringboardLaunchesTheBodyToTheSpringboardsHeight()
        {
            var springboard = _physics.Box("Springboard", new Vector2(0f, 3.5f), new Vector2(4f, 1f));
            springboard.gameObject.AddComponent<Springboard>().With("_launchHeight", 3f);
            var definition = PhysicsShapeDefinition.defaultDefinition;
            definition.contactEvents = true;
            _physics.Space.CreateBody(_physics.Lifetime, springboard, new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Static }, definition);
            var launch = Mathf.Sqrt(-2f * Physics2D.gravity.y * _body.gravityScale * 3f);
            var fastest = 0f;

            for (var tick = 0; tick < 30; tick++)
            {
                _physics.Step();
                fastest = Mathf.Max(fastest, _crate.State.Value.Velocity.y);
            }

            Assert.That(fastest, Is.EqualTo(launch).Within(1e-3f));
        }
    }
}
