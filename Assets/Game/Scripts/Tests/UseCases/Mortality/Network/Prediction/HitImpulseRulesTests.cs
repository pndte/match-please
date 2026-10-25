using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Network.Prediction;
using Bw.UseCases.Movement;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Mortality.Network.Prediction
{
    public sealed class HitImpulseRulesTests
    {
        private const float KnockbackMass = 2f;
        private const float Spin = 0.5f;

        private static readonly RigidState Resting = new(new Vector2(4f, -1f), 0f, new Vector2(1f, 0f), 5f);

        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private HitImpulseConfig _config;
        private PhysicsBody _body;
        private HitImpulseRules _rules;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            var space = new TestSpace(_lifetime.Lifetime, _objects);
            _body = space.Body(space.Box("crate", Vector2.zero, new Vector2(1f, 2f), new Vector2(0f, 1f)));
            _config = ScriptableObject.CreateInstance<HitImpulseConfig>();
            _config.KnockbackMass = KnockbackMass;
            _config.Spin = Spin;
            _rules = new HitImpulseRules(_body, _config);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void BlowAtTheCentreOfMassOnlyPushes()
        {
            var state = _rules.Apply(Resting, Blows.Of(Blows.Pushing(new Vector2(3f, 0f), new Vector2(0f, 1f))));

            Assert.That(state.Velocity.x, Is.EqualTo(1f + 3f * KnockbackMass / _body.mass).Within(1e-4f));
            Assert.That(state.Velocity.y, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(state.AngularVelocity, Is.EqualTo(5f).Within(1e-4f));
        }

        [Test]
        public void BlowAboveTheCentrePushingRightSpinsClockwise()
        {
            var expected = 5f - 1f * 3f * KnockbackMass / _body.rotationalInertia * Spin * Mathf.Rad2Deg;

            var state = _rules.Apply(Resting, Blows.Of(Blows.Pushing(new Vector2(3f, 0f), new Vector2(0f, 2f))));

            Assert.That(state.AngularVelocity, Is.EqualTo(expected).Within(1e-3f));
            Assert.That(state.AngularVelocity, Is.LessThan(5f));
        }

        [Test]
        public void EveryImpactAddsItsOwnImpulseAtItsOwnPoint()
        {
            var impacts = Blows.Of(
                Blows.Pushing(new Vector2(3f, 0f), new Vector2(0f, 2f)),
                Blows.Pushing(new Vector2(3f, 0f), new Vector2(0f, 0f)));

            var state = _rules.Apply(Resting, impacts);

            Assert.That(state.Velocity.x, Is.EqualTo(1f + 6f * KnockbackMass / _body.mass).Within(1e-4f));
            Assert.That(state.AngularVelocity, Is.EqualTo(5f).Within(1e-3f));
        }

        [Test]
        public void TurnedBodyTurnsTheContactWithIt()
        {
            var turned = new RigidState(Vector2.zero, 90f, Vector2.zero, 0f);

            var sideways = _rules.Apply(turned, Blows.Of(Blows.Pushing(new Vector2(3f, 0f), new Vector2(0f, 2f))));
            var upwards = _rules.Apply(turned, Blows.Of(Blows.Pushing(new Vector2(0f, 3f), new Vector2(0f, 2f))));

            Assert.That(sideways.AngularVelocity, Is.EqualTo(0f).Within(1e-3f));
            Assert.That(upwards.AngularVelocity, Is.EqualTo(-3f * KnockbackMass / _body.rotationalInertia * Spin * Mathf.Rad2Deg).Within(1e-3f));
        }

        [Test]
        public void BlowKeepsThePoseOfTheBody()
        {
            var state = _rules.Apply(new RigidState(new Vector2(4f, -1f), 30f, Vector2.zero, 0f), Blows.Of(Blows.Pushing(Vector2.one, Vector2.one)));

            Assert.That(state.Position, Is.EqualTo(new Vector2(4f, -1f)));
            Assert.That(state.Angle, Is.EqualTo(30f));
        }

        [Test]
        public void NoImpactsChangeNothing()
        {
            Assert.That(_rules.Apply(Resting, default), Is.EqualTo(Resting));
        }
    }
}
