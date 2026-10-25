using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Tests.Doubles;
using NUnit.Framework;

namespace Bw.Tests.Entities.Extensions
{
    public sealed class GameObjectExtensionsTests
    {
        private HiddenObjects _objects;

        [SetUp]
        public void SetUp() =>
            _objects = new HiddenObjects();

        [TearDown]
        public void TearDown() =>
            _objects.Dispose();

        [Test]
        public void LifetimeOfAnExistingObjectIsAlive()
        {
            var gameObject = _objects.Create("owner");

            Assert.That(gameObject.Lifetime().IsAlive, Is.True);
        }

        [Test]
        public void ObjectKeepsOneLifetime()
        {
            var gameObject = _objects.Create("owner");

            var first = gameObject.Lifetime();
            var second = gameObject.Lifetime();

            Assert.That(second, Is.EqualTo(first));
            Assert.That(gameObject.GetComponents<LifetimeHolder>(), Has.Length.EqualTo(1));
        }

        [Test]
        public void HolderAlreadyOnTheObjectGivesItsLifetime()
        {
            var gameObject = _objects.Create("owner");
            var holder = gameObject.AddComponent<LifetimeHolder>();

            Assert.That(gameObject.Lifetime(), Is.EqualTo(holder.GameObjectLifetime));
            Assert.That(gameObject.GetComponents<LifetimeHolder>(), Has.Length.EqualTo(1));
        }

        [Test]
        public void ObjectsHaveTheirOwnLifetimes()
        {
            var first = _objects.Create("first");
            var second = _objects.Create("second");

            Assert.That(first.Lifetime(), Is.Not.EqualTo(second.Lifetime()));
        }
    }
}
