using Bw.Entities.Physics.Extensions;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.UseCases.Level;
using Bw.UseCases.Movement.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Movement.Extensions
{
    public sealed class SpringboardBodyExtensionsTests
    {
        private const float LaunchHeight = 2f;

        private PhysicsTestWorld _physics;

        [SetUp]
        public void SetUp() =>
            _physics = new PhysicsTestWorld();

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [Test]
        public void BodyLandingOnASpringboardGetsTheSpeedThatReachesItsHeight()
        {
            SpringboardAt(Vector2.zero, new Vector2(4f, 1f));
            var body = _physics.Dynamic("Crate", new Vector2(0f, 1.5f), Vector2.one);
            body.gravityScale = 2f;

            var launched = StepUntilLaunched(body, 30, out var speed);

            Assert.That(launched, Is.True);
            Assert.That(speed, Is.EqualTo(Mathf.Sqrt(-2f * Physics2D.gravity.y * 2f * LaunchHeight)).Within(1e-4f));
        }

        [Test]
        public void LaunchIsOncePerLanding()
        {
            SpringboardAt(Vector2.zero, new Vector2(4f, 1f));
            var body = _physics.Dynamic("Crate", new Vector2(0f, 1.5f), Vector2.one);
            StepUntilLaunched(body, 30, out _);

            var again = StepUntilLaunched(body, 10, out _);

            Assert.That(again, Is.False);
        }

        [Test]
        public void PlainFloorLaunchesNothing()
        {
            _physics.Space.CreateBody(_physics.Lifetime, _physics.Box("Floor", Vector2.zero, new Vector2(4f, 1f)), StaticBody(), ContactEvents());
            var body = _physics.Dynamic("Crate", new Vector2(0f, 1.5f), Vector2.one);

            Assert.That(StepUntilLaunched(body, 30, out _), Is.False);
        }

        [Test]
        public void HittingTheSpringboardsSideLaunchesNothing()
        {
            SpringboardAt(Vector2.zero, new Vector2(1f, 4f));
            var body = _physics.Dynamic("Crate", new Vector2(-2f, 0f), Vector2.one);
            body.gravityScale = 0f;
            body.linearVelocity = new Vector2(10f, 0f);

            Assert.That(StepUntilLaunched(body, 30, out _), Is.False);
        }

        [Test]
        public void AnotherBodysLandingLaunchesOnlyIt()
        {
            SpringboardAt(Vector2.zero, new Vector2(4f, 1f));
            var lander = _physics.Dynamic("Lander", new Vector2(0f, 1.5f), Vector2.one);
            var bystander = _physics.Dynamic("Bystander", new Vector2(50f, 0f), Vector2.one);
            bystander.gravityScale = 0f;

            for (var tick = 0; tick < 30; tick++)
            {
                _physics.Step();
                if (lander.TryGetLaunchSpeed(out _))
                    break;
            }

            Assert.That(bystander.TryGetLaunchSpeed(out var speed), Is.False);
            Assert.That(speed, Is.EqualTo(0f));
        }

        private void SpringboardAt(Vector2 position, Vector2 size)
        {
            var box = _physics.Box("Springboard", position, size);
            box.gameObject.AddComponent<Springboard>().With("_launchHeight", LaunchHeight);
            _physics.Space.CreateBody(_physics.Lifetime, box, StaticBody(), ContactEvents());
        }

        private bool StepUntilLaunched(PhysicsBody body, int ticks, out float speed)
        {
            for (var tick = 0; tick < ticks; tick++)
            {
                _physics.Step();
                if (body.TryGetLaunchSpeed(out speed))
                    return true;
            }

            speed = 0f;
            return false;
        }

        private static PhysicsBodyDefinition StaticBody()
        {
            var body = PhysicsBodyDefinition.defaultDefinition;
            body.type = PhysicsBody.BodyType.Static;
            return body;
        }

        private static PhysicsShapeDefinition ContactEvents()
        {
            var shape = PhysicsShapeDefinition.defaultDefinition;
            shape.contactEvents = true;
            return shape;
        }
    }
}
