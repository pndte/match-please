using System;
using System.Collections.Generic;
using Bw.Entities.Pool;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Pool
{
    public sealed class PoolExtensionsTests
    {
        private LifetimeDefinition _lifetime;
        private List<Shot> _created;
        private LimitedPool<Shot> _pool;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _created = new List<Shot>();
            _pool = LimitedPool<Shot>.Create(_lifetime.Lifetime, Create, new PoolSettings(0, 4, PoolCap.None));
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void OneShotLendsTheFacade()
        {
            var shot = _pool.OneShot(_lifetime.Lifetime);

            Assert.That(_created, Is.EqualTo(new[] { shot }));
            Assert.That(shot.Borrow.IsAlive, Is.True);
        }

        [Test]
        public void OneShotEndsWhenTheFacadeFinishes()
        {
            var shot = _pool.OneShot(_lifetime.Lifetime);

            shot.Finish();

            Assert.That(shot.Borrow.IsAlive, Is.False);
            Assert.That(_lifetime.Lifetime.IsAlive, Is.True);
        }

        [Test]
        public void FinishedShotReturnsToThePool()
        {
            var first = _pool.OneShot(_lifetime.Lifetime);
            first.Finish();

            var second = _pool.OneShot(_lifetime.Lifetime);

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.Borrow.IsAlive, Is.True);
        }

        [Test]
        public void OneShotEndsWithItsLifetime()
        {
            var lifetime = _lifetime.Lifetime.CreateNested();
            var shot = _pool.OneShot(lifetime.Lifetime);

            lifetime.Terminate();

            Assert.That(shot.Borrow.IsAlive, Is.False);
        }

        [Test]
        public void FinishEndsOnlyItsOwnShot()
        {
            var first = _pool.OneShot(_lifetime.Lifetime);
            var second = _pool.OneShot(_lifetime.Lifetime);

            first.Finish();

            Assert.That(first.Borrow.IsAlive, Is.False);
            Assert.That(second.Borrow.IsAlive, Is.True);
        }

        [Test]
        public void ReusedShotFinishesItsNewBorrow()
        {
            var shot = _pool.OneShot(_lifetime.Lifetime);
            shot.Finish();
            _pool.OneShot(_lifetime.Lifetime);

            shot.Finish();

            Assert.That(shot.Borrow.IsAlive, Is.False);
        }

        [Test]
        public void RefusedBorrowEndsTheShotAndRethrows()
        {
            var pool = new RefusingPool();

            Assert.Throws<InvalidOperationException>(() => pool.OneShot(_lifetime.Lifetime));
            Assert.That(pool.Asked.IsAlive, Is.False);
            Assert.That(_lifetime.Lifetime.IsAlive, Is.True);
        }

        private IResource<Shot> Create(Lifetime life)
        {
            var shot = new Shot();
            _created.Add(shot);
            return shot;
        }

        private sealed class Shot : IResource<Shot>, IOneShot
        {
            public ISource<Unit> Finished => _finished;
            public Lifetime Borrow { get; private set; } = Lifetime.Terminated;

            private readonly Signal<Unit> _finished = new();

            public Shot Facade(Lifetime lifetime)
            {
                Borrow = lifetime;
                return this;
            }

            public void Finish() =>
                _finished.Fire(Unit.Instance);
        }

        private sealed class RefusingPool : IPool<Shot>
        {
            public Lifetime Asked { get; private set; }

            public Shot Resource(Lifetime lifetime)
            {
                Asked = lifetime;
                throw new InvalidOperationException("No room.");
            }
        }
    }
}
