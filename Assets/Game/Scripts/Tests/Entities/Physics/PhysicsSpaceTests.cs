using Bw.Entities.Physics;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Physics.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.Entities.Physics
{
    public sealed class PhysicsSpaceTests
    {
        private PhysicsTestWorld _physics;

        [SetUp]
        public void SetUp() =>
            _physics = new PhysicsTestWorld();

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [Test]
        public void CreatedWorldIsSteppedByScriptWithTheProjectGravityAndTheConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<PhysicsConfig>(PhysicsTestWorld.ConfigPath);
            var world = _physics.World;

            Assert.That(world.isValid, Is.True);
            Assert.That(world.simulationType, Is.EqualTo(PhysicsWorld.SimulationType.Script));
            Assert.That(world.gravity, Is.EqualTo(Physics2D.gravity));
            Assert.That(world.simulationSubSteps, Is.EqualTo(config.SubSteps));
            Assert.That(world.sleepingAllowed, Is.EqualTo(config.Sleeping));
            Assert.That(world.warmStartingAllowed, Is.EqualTo(config.WarmStarting));
            Assert.That(world.contactFrequency, Is.EqualTo(config.ContactFrequency));
            Assert.That(world.transformWriteMode, Is.EqualTo(PhysicsWorld.TransformWriteMode.Off));
        }

        [Test]
        public void WorldDiesWithTheLifetime()
        {
            var lifetime = new LifetimeDefinition();
            var space = PhysicsSpace.Create(lifetime.Lifetime, AssetDatabase.LoadAssetAtPath<PhysicsConfig>(PhysicsTestWorld.ConfigPath), new ManualClock());
            var world = space.World;

            lifetime.Terminate();

            Assert.That(world.isValid, Is.False);
        }

        [Test]
        public void StepSimulatesOneTickAndAnnouncesIt()
        {
            var body = _physics.Dynamic("Falling", new Vector2(0f, 10f), Vector2.one);
            var stepped = 0;
            _physics.Space.Stepped.Advise(_physics.Lifetime, _ => stepped++);

            _physics.Space.Step();

            Assert.That(stepped, Is.EqualTo(1));
            Assert.That(body.linearVelocity.y, Is.EqualTo(Physics2D.gravity.y * _physics.Clock.Duration).Within(1e-4f));
            Assert.That(body.position.y, Is.LessThan(10f));
        }

        [Test]
        public void StepWithoutBodiesStillAnnouncesTheTick()
        {
            var stepped = 0;
            _physics.Space.Stepped.Advise(_physics.Lifetime, _ => stepped++);

            _physics.Step(3);

            Assert.That(stepped, Is.EqualTo(3));
        }

        [Test]
        public void LinearSlopIsThePhysicsWorldsLinearSlop()
        {
            Assert.That(PhysicsSlop.Linear, Is.EqualTo(PhysicsWorld.linearSlop).Within(1e-7f));
        }
    }
}
