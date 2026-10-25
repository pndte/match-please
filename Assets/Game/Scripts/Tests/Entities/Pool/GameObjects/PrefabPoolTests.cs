using System;
using System.Collections.Generic;
using Bw.Entities.Pool;
using Bw.Entities.Pool.GameObjects;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Pool.GameObjects
{
    public sealed class PrefabPoolTests
    {
        [Test]
        public void ResourceIsTheFacadeOfAnInstanceBorrowedForTheSameLifetime()
        {
            var instances = new Instances("facade");
            var pool = new PrefabPool<string>(instances);
            using var borrow = new LifetimeDefinition();

            var resource = pool.Resource(borrow.Lifetime);

            Assert.That(resource, Is.EqualTo("facade"));
            Assert.That(instances.Borrows, Is.EqualTo(new[] { borrow.Lifetime }));
            Assert.That(instances.AskedFacades, Is.EqualTo(new[] { typeof(string) }));
        }

        [Test]
        public void EveryResourceBorrowsItsOwnInstance()
        {
            var instances = new Instances("facade");
            var pool = new PrefabPool<string>(instances);

            pool.Resource(Lifetime.Eternal);
            pool.Resource(Lifetime.Eternal);

            Assert.That(instances.Borrows, Has.Count.EqualTo(2));
        }

        [Test]
        public void RefusedInstanceIsRefusedResource()
        {
            var pool = new PrefabPool<string>(new RefusingInstances());

            Assert.Throws<InvalidOperationException>(() => pool.Resource(Lifetime.Eternal));
        }

        private sealed class Instances : IPool<IPrefabInstance>, IPrefabInstance
        {
            public List<Lifetime> Borrows { get; } = new();
            public List<Type> AskedFacades { get; } = new();

            private readonly object _facade;

            public Instances(object facade)
            {
                _facade = facade;
            }

            public IPrefabInstance Resource(Lifetime lifetime)
            {
                Borrows.Add(lifetime);
                return this;
            }

            public T Facade<T>()
            {
                AskedFacades.Add(typeof(T));
                return (T)_facade;
            }
        }

        private sealed class RefusingInstances : IPool<IPrefabInstance>
        {
            public IPrefabInstance Resource(Lifetime lifetime) =>
                throw new InvalidOperationException("No room.");
        }
    }
}
