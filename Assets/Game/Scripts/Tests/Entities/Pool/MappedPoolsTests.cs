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
            var pools = Typed(new Dictionary<Type, IPool<object>> { [typeof(string)] = strings });

            Assert.That(pools.For<string>(), Is.SameAs(strings));
        }

        [Test]
        public void PoolsOfDifferentFacadesAreKeptApart()
        {
            var strings = new FixedPool<string>("text");
            var arrays = new FixedPool<int[]>(new[] { 1 });
            var pools = Typed(new Dictionary<Type, IPool<object>>
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
            var pools = Typed(new Dictionary<Type, IPool<object>> { [typeof(string)] = new FixedPool<string>("text") });

            Assert.Throws<KeyNotFoundException>(() => pools.For<int[]>());
        }

        [Test]
        public void BaseTypeDoesNotStandForItsSubtype()
        {
            var pools = Typed(new Dictionary<Type, IPool<object>> { [typeof(object)] = new FixedPool<object>("text") });

            Assert.Throws<KeyNotFoundException>(() => pools.For<string>());
        }

        [Test]
        public void KeyedPoolsAreFoundByTheKeyType()
        {
            var key = new Key();
            var pools = Keyed(new Dictionary<Type, object> { [typeof(Key)] = new Family<Key>("keys") });

            Assert.That(pools.For<string, Key>(key).Resource(Lifetime.Eternal), Is.EqualTo("keys " + key.Name));
        }

        [Test]
        public void KeyedPoolsTakeTheFacadeTheCallerAsksFor()
        {
            var pools = Keyed(new Dictionary<Type, object> { [typeof(Key)] = new Family<Key>("keys") });

            Assert.That(pools.For<string, Key>(new Key()), Is.InstanceOf<KeyPool<string>>());
            Assert.That(pools.For<int[], Key>(new Key()), Is.InstanceOf<KeyPool<int[]>>());
        }

        [Test]
        public void PoolsKeyedByABaseTypeServeItsSubtype()
        {
            var key = new SubKey();
            var pools = Keyed(new Dictionary<Type, object> { [typeof(Key)] = new Family<Key>("keys") });

            Assert.That(pools.For<string, SubKey>(key).Resource(Lifetime.Eternal), Is.EqualTo("keys " + key.Name));
        }

        [Test]
        public void TheNearestBaseTypeWins()
        {
            var pools = Keyed(new Dictionary<Type, object>
            {
                [typeof(Key)] = new Family<Key>("keys"),
                [typeof(SubKey)] = new Family<SubKey>("subkeys")
            });

            Assert.That(pools.For<string, LeafKey>(new LeafKey()).Resource(Lifetime.Eternal), Does.StartWith("subkeys "));
        }

        [Test]
        public void PoolsKeyedByAnInterfaceServeItsImplementation()
        {
            var key = new MarkedKey();
            var pools = Keyed(new Dictionary<Type, object> { [typeof(IMark)] = new Family<IMark>("marks") });

            Assert.That(pools.For<string, MarkedKey>(key).Resource(Lifetime.Eternal), Is.EqualTo("marks " + key.Name));
        }

        [Test]
        public void ABaseClassWinsOverAnInterface()
        {
            var pools = Keyed(new Dictionary<Type, object>
            {
                [typeof(IMark)] = new Family<IMark>("marks"),
                [typeof(Key)] = new Family<Key>("keys")
            });

            Assert.That(pools.For<string, MarkedKey>(new MarkedKey()).Resource(Lifetime.Eternal), Does.StartWith("keys "));
        }

        [Test]
        public void TwoMatchingInterfacesThrow()
        {
            var pools = Keyed(new Dictionary<Type, object>
            {
                [typeof(IMark)] = new Family<IMark>("marks"),
                [typeof(ITag)] = new Family<ITag>("tags")
            });

            Assert.Throws<InvalidOperationException>(() => pools.For<string, MarkedKey>(new MarkedKey()));
        }

        [Test]
        public void SubtypeDoesNotStandForItsBase()
        {
            var pools = Keyed(new Dictionary<Type, object> { [typeof(SubKey)] = new Family<SubKey>("subkeys") });

            Assert.Throws<KeyNotFoundException>(() => pools.For<string, Key>(new SubKey()));
        }

        [Test]
        public void KeyTypeWithoutPoolsThrows()
        {
            var pools = Keyed(new Dictionary<Type, object> { [typeof(Key)] = new Family<Key>("keys") });

            Assert.Throws<KeyNotFoundException>(() => pools.For<string, string>("key"));
        }

        [Test]
        public void StructKeysAreFoundByTheirType()
        {
            var pools = Keyed(new Dictionary<Type, object> { [typeof(int)] = new Family<int>("lengths") });

            Assert.That(pools.For<string, int>(16).Resource(Lifetime.Eternal), Is.EqualTo("lengths 16"));
        }

        [Test]
        public void ResolvedKeyTypesStayApart()
        {
            var pools = Keyed(new Dictionary<Type, object>
            {
                [typeof(Key)] = new Family<Key>("keys"),
                [typeof(SubKey)] = new Family<SubKey>("subkeys")
            });

            Assert.That(pools.For<string, LeafKey>(new LeafKey()).Resource(Lifetime.Eternal), Does.StartWith("subkeys "));
            Assert.That(pools.For<string, Key>(new Key()).Resource(Lifetime.Eternal), Does.StartWith("keys "));
            Assert.That(pools.For<string, LeafKey>(new LeafKey()).Resource(Lifetime.Eternal), Does.StartWith("subkeys "));
        }

        private static MappedPools Typed(Dictionary<Type, IPool<object>> pools) =>
            new(pools, new Dictionary<Type, object>());

        private static MappedPools Keyed(Dictionary<Type, object> keyed) =>
            new(new Dictionary<Type, IPool<object>>(), keyed);

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

        private sealed class Family<TKey> : IKeyedPools<TKey>
        {
            private readonly string _name;

            public Family(string name)
            {
                _name = name;
            }

            public IPool<T> For<T>(TKey key) where T : class =>
                new KeyPool<T>($"{_name} {key}");
        }

        private sealed class KeyPool<T> : IPool<T> where T : class
        {
            private readonly string _label;

            public KeyPool(string label)
            {
                _label = label;
            }

            public T Resource(Lifetime lifetime) =>
                (T)(object)_label;
        }

        private interface IMark
        {
        }

        private interface ITag
        {
        }

        private class Key
        {
            public string Name { get; } = Guid.NewGuid().ToString("N");

            public override string ToString() =>
                Name;
        }

        private class SubKey : Key
        {
        }

        private sealed class LeafKey : SubKey
        {
        }

        private sealed class MarkedKey : Key, IMark, ITag
        {
        }
    }
}
