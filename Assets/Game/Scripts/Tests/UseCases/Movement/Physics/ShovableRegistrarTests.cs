using Bw.Tests.Entities.Physics.Doubles;
using Bw.Tests.UseCases.Movement.Doubles;
using Bw.UseCases.Movement.Physics;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Movement.Physics
{
    public sealed class ShovableRegistrarTests
    {
        private PhysicsTestWorld _physics;
        private ShovableCollection _shovables;
        private PhysicsBody _body;
        private RecordedShovable _crate;

        [SetUp]
        public void SetUp()
        {
            _physics = new PhysicsTestWorld();
            _shovables = new ShovableCollection();
            _body = _physics.Dynamic("Crate", Vector2.zero, Vector2.one);
            _crate = new RecordedShovable();
        }

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [Test]
        public void RegisteredShovableIsFoundByItsBody()
        {
            new ShovableRegistrar(_physics.Lifetime, _body, _crate, _shovables);

            Assert.That(_shovables.TryGetValue(_body, out var found), Is.True);
            Assert.That(found, Is.SameAs(_crate));
        }

        [Test]
        public void ShovableLeavesWithItsLifetime()
        {
            var lifetime = new LifetimeDefinition();
            new ShovableRegistrar(lifetime.Lifetime, _body, _crate, _shovables);

            lifetime.Terminate();

            Assert.That(_shovables.ContainsKey(_body), Is.False);
        }
    }
}
