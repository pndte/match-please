using Bw.Entities;
using Bw.Entities.Pool.GameObjects;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Doubles;
using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.Entities.Pool.GameObjects
{
    public sealed class PrefabInstanceTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void ComponentWithoutALenderIsItsOwnFacade()
        {
            var gameObject = _objects.Create("instance");
            var component = gameObject.AddComponent<LifetimeHolder>();
            var instance = new PrefabInstance(_lifetime.Lifetime, gameObject);

            Assert.That(instance.Facade<LifetimeHolder>(), Is.SameAs(component));
        }

        [Test]
        public void EngineComponentIsGivenAsItIs()
        {
            var gameObject = _objects.Create("instance");
            var instance = new PrefabInstance(_lifetime.Lifetime, gameObject);

            Assert.That(instance.Facade<Transform>(), Is.SameAs(gameObject.transform));
        }

        [Test]
        public void LenderWinsOverTheComponentAndLendsForTheInstanceLifetime()
        {
            var gameObject = _objects.Create("instance");
            var lender = FacadeLender.On(gameObject, _objects);
            var borrow = _lifetime.Lifetime.CreateNested();
            var instance = new PrefabInstance(borrow.Lifetime, gameObject);

            var facade = instance.Facade<IBulletTracer>();
            var hiddenWhileBorrowed = lender.Hidden;
            borrow.Terminate();

            Assert.That(facade, Is.SameAs(lender.Tracer));
            Assert.That(hiddenWhileBorrowed, Is.False);
            Assert.That(lender.Hidden, Is.True);
        }
    }
}
