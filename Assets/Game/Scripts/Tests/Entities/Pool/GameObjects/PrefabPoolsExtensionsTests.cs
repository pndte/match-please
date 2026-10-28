using System;
using Bw.Entities.Pool;
using Bw.Entities.Pool.GameObjects;
using Bw.Tests.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.Entities.Pool.GameObjects
{
    public sealed class PrefabPoolsExtensionsTests
    {
        private HiddenObjects _objects;

        [SetUp]
        public void SetUp()
        {
            _objects = new HiddenObjects();
        }

        [TearDown]
        public void TearDown()
        {
            _objects.Dispose();
        }

        [Test]
        public void PrefabIsTheKeyOfThePool()
        {
            var prefab = _objects.Create("prefab");
            var pools = new RecordingPools();

            pools.For<string>(prefab);

            Assert.That(pools.KeyType, Is.EqualTo(typeof(GameObject)));
            Assert.That(pools.Key, Is.SameAs(prefab));
            Assert.That(pools.Facade, Is.EqualTo(typeof(string)));
        }

        private sealed class RecordingPools : IPools
        {
            public Type KeyType { get; private set; }
            public object Key { get; private set; }
            public Type Facade { get; private set; }

            public IPool<T> For<T>() where T : class =>
                throw new InvalidOperationException("The prefab extension asked for a pool without a key.");

            public IPool<T> For<T, TKey>(TKey key) where T : class
            {
                KeyType = typeof(TKey);
                Key = key;
                Facade = typeof(T);
                return new EmptyPool<T>();
            }
        }

        private sealed class EmptyPool<T> : IPool<T>
        {
            public T Resource(Lifetime lifetime) =>
                throw new InvalidOperationException("The test takes nothing from the pool.");
        }
    }
}
