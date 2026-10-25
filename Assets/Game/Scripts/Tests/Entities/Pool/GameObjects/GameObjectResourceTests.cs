using Bw.Entities.Pool.GameObjects;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Doubles;
using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.Entities.Pool.GameObjects
{
    public sealed class GameObjectResourceTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private GameObject _gameObject;
        private LifetimeDefinition _gameObjectLifetime;
        private GameObjectResource _resource;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _gameObject = _objects.Create("pooled");
            _gameObject.SetActive(false);
            _gameObjectLifetime = _lifetime.Lifetime.CreateNested();
            _resource = new GameObjectResource(_gameObject, _gameObjectLifetime.Lifetime);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void BorrowedObjectIsSwitchedOn()
        {
            _resource.Facade(_lifetime.Lifetime);

            Assert.That(_gameObject.activeSelf, Is.True);
        }

        [Test]
        public void ObjectIsSwitchedOffWhenTheBorrowEnds()
        {
            var borrow = _lifetime.Lifetime.CreateNested();
            _resource.Facade(borrow.Lifetime);

            borrow.Terminate();

            Assert.That(_gameObject.activeSelf, Is.False);
        }

        [Test]
        public void ObjectIsSwitchedOnAgainForTheNextBorrow()
        {
            var first = _lifetime.Lifetime.CreateNested();
            _resource.Facade(first.Lifetime);
            first.Terminate();

            _resource.Facade(_lifetime.Lifetime);

            Assert.That(_gameObject.activeSelf, Is.True);
        }

        [Test]
        public void ObjectThatEndedItsOwnLifeIsLeftAloneWhenTheBorrowEnds()
        {
            var borrow = _lifetime.Lifetime.CreateNested();
            _resource.Facade(borrow.Lifetime);
            _gameObjectLifetime.Terminate();

            borrow.Terminate();

            Assert.That(_gameObject.activeSelf, Is.True);
        }

        [Test]
        public void InstanceGivesFacadesForTheBorrow()
        {
            var lender = FacadeLender.On(_gameObject, _objects);
            var borrow = _lifetime.Lifetime.CreateNested();
            var instance = _resource.Facade(borrow.Lifetime);

            var facade = instance.Facade<IBulletTracer>();
            var hiddenWhileBorrowed = lender.Hidden;
            borrow.Terminate();

            Assert.That(facade, Is.SameAs(lender.Tracer));
            Assert.That(hiddenWhileBorrowed, Is.False);
            Assert.That(lender.Hidden, Is.True);
        }
    }
}
