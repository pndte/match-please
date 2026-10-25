using System;
using System.Collections.Generic;
using Bw.Entities.Pool;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Pool
{
    public sealed class MappedPoolsTests
    {
        [Test]
        public void PoolIsFoundByItsFacadeType()
        {
            var strings = new FixedPool<string>("text");
            var pools = new MappedPools(new Dictionary<Type, IPool<object>> { [typeof(string)] = strings });

            Assert.That(pools.For<string>(), Is.SameAs(strings));
        }

        [Test]
        public void PoolsOfDifferentFacadesAreKeptApart()
        {
            var strings = new FixedPool<string>("text");
            var arrays = new FixedPool<int[]>(new[] { 1 });
            var pools = new MappedPools(new Dictionary<Type, IPool<object>>
            {
                [typeof(string)] = strings,
                [typeof(int[])] = arrays
            });

            Assert.That(pools.For<string>().Resource(Lifetime.Eternal), Is.EqualTo("text"));
            Assert.That(pools.For<int[]>().Resource(Lifetime.Eternal), Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void FacadeWithoutAPoolThrows()
        {
            var pools = new MappedPools(new Dictionary<Type, IPool<object>> { [typeof(string)] = new FixedPool<string>("text") });

            Assert.Throws<KeyNotFoundException>(() => pools.For<int[]>());
        }

        [Test]
        public void BaseTypeDoesNotStandForItsSubtype()
        {
            var pools = new MappedPools(new Dictionary<Type, IPool<object>> { [typeof(object)] = new FixedPool<object>("text") });

            Assert.Throws<KeyNotFoundException>(() => pools.For<string>());
        }

        private sealed class FixedPool<T> : IPool<T>
        {
            private readonly T _value;

            public FixedPool(T value)
            {
                _value = value;
            }

            public T Resource(Lifetime lifetime) =>
                _value;
        }
    }
}
