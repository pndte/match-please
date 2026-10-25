using System;
using System.Collections.Generic;
using Bw.Entities.Pool;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Pool
{
    public sealed class LimitedPoolTests
    {
        private LifetimeDefinition _lifetime;
        private LifetimeDefinition _poolLifetime;
        private List<Item> _created;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _poolLifetime = _lifetime.Lifetime.CreateNested();
            _created = new List<Item>();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void CreatedPoolBuildsNothingUntilAsked()
        {
            Pool(prewarm: 2, maxIdle: 4);

            Assert.That(_created, Is.Empty);
        }

        [Test]
        public void FirstBorrowCreatesAResourceInsideThePool()
        {
            var pool = Pool();

            var item = pool.Resource(Borrower());

            Assert.That(_created, Is.EqualTo(new[] { item }));
            Assert.That(item.Life.IsAlive, Is.True);
        }

        [Test]
        public void FacadeGetsTheBorrowThatEndsWithTheBorrower()
        {
            var pool = Pool();
            var borrower = _lifetime.Lifetime.CreateNested();

            var item = pool.Resource(borrower.Lifetime);
            var borrowedWhileAlive = item.Borrow.IsAlive;
            borrower.Terminate();

            Assert.That(borrowedWhileAlive, Is.True);
            Assert.That(item.Borrow.IsAlive, Is.False);
            Assert.That(item.Life.IsAlive, Is.True);
        }

        [Test]
        public void ReturnedResourceIsLentAgain()
        {
            var pool = Pool();
            var borrower = _lifetime.Lifetime.CreateNested();
            var first = pool.Resource(borrower.Lifetime);
            borrower.Terminate();

            var second = pool.Resource(Borrower());

            Assert.That(second, Is.SameAs(first));
            Assert.That(_created, Has.Count.EqualTo(1));
            Assert.That(second.Borrow.IsAlive, Is.True);
        }

        [Test]
        public void ResourceInUseIsNeverLentTwice()
        {
            var pool = Pool();

            var first = pool.Resource(Borrower());
            var second = pool.Resource(Borrower());

            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(_created, Has.Count.EqualTo(2));
        }

        [Test]
        public void OneBorrowerMayHoldSeveralResources()
        {
            var pool = Pool();
            var borrower = Borrower();

            var first = pool.Resource(borrower);
            var second = pool.Resource(borrower);

            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void TheLastReturnedResourceIsLentFirst()
        {
            var pool = Pool();
            var firstBorrower = _lifetime.Lifetime.CreateNested();
            var secondBorrower = _lifetime.Lifetime.CreateNested();
            var first = pool.Resource(firstBorrower.Lifetime);
            var second = pool.Resource(secondBorrower.Lifetime);
            firstBorrower.Terminate();
            secondBorrower.Terminate();

            var next = pool.Resource(Borrower());

            Assert.That(next, Is.SameAs(second));
            Assert.That(first.Life.IsAlive, Is.True);
        }

        [Test]
        public void ResourceReturnedOverMaxIdleIsDestroyed()
        {
            var pool = Pool(maxIdle: 1);
            var firstBorrower = _lifetime.Lifetime.CreateNested();
            var secondBorrower = _lifetime.Lifetime.CreateNested();
            var first = pool.Resource(firstBorrower.Lifetime);
            var second = pool.Resource(secondBorrower.Lifetime);

            firstBorrower.Terminate();
            secondBorrower.Terminate();

            Assert.That(first.Life.IsAlive, Is.True);
            Assert.That(second.Life.IsAlive, Is.False);
        }

        [Test]
        public void DestroyedResourceIsNeverLentAgain()
        {
            var pool = Pool(maxIdle: 0);
            var borrower = _lifetime.Lifetime.CreateNested();
            var first = pool.Resource(borrower.Lifetime);
            borrower.Terminate();

            var second = pool.Resource(Borrower());

            Assert.That(first.Life.IsAlive, Is.False);
            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void EndedPoolDestroysEveryResourceAndEndsEveryBorrow()
        {
            var pool = Pool(prewarm: 2, maxIdle: 2);
            pool.Prewarm();
            var lent = pool.Resource(Borrower());

            _poolLifetime.Terminate();

            Assert.That(_created, Has.Count.EqualTo(2));
            Assert.That(_created.TrueForAll(item => item.Life.IsNotAlive), Is.True);
            Assert.That(lent.Borrow.IsAlive, Is.False);
        }

        [Test]
        public void BorrowerEndingAfterThePoolChangesNothing()
        {
            var pool = Pool();
            var borrower = _lifetime.Lifetime.CreateNested();
            var item = pool.Resource(borrower.Lifetime);
            _poolLifetime.Terminate();

            borrower.Terminate();

            Assert.That(item.Life.IsAlive, Is.False);
            Assert.That(_created, Has.Count.EqualTo(1));
        }

        [Test]
        public void EndedPoolRefusesToLend()
        {
            var pool = Pool();
            _poolLifetime.Terminate();

            Assert.Throws<LifetimeCanceledException>(() => pool.Resource(Borrower()));
            Assert.That(_created, Is.Empty);
        }

        [Test]
        public void EndedBorrowerIsRefused()
        {
            var pool = Pool();

            Assert.Throws<LifetimeCanceledException>(() => pool.Resource(Lifetime.Terminated));
            Assert.That(_created, Is.Empty);
        }

        [Test]
        public void PoolCannotBeCreatedOnAnEndedLifetime()
        {
            _poolLifetime.Terminate();

            Assert.Throws<LifetimeCanceledException>(() => Pool());
        }

        [Test]
        public void PrewarmOverMaxIdleIsRefused()
        {
            Assert.Throws<ArgumentException>(() => Pool(prewarm: 3, maxIdle: 2));
        }

        [Test]
        public void PrewarmEqualToMaxIdleIsAllowed()
        {
            Assert.DoesNotThrow(() => Pool(prewarm: 2, maxIdle: 2));
        }

        [Test]
        public void PrewarmFillsTheFreeResourcesUpToTheSetting()
        {
            var pool = Pool(prewarm: 3, maxIdle: 4);

            pool.Prewarm();

            Assert.That(_created, Has.Count.EqualTo(3));
            Assert.That(_created.TrueForAll(item => item.Life.IsAlive), Is.True);
        }

        [Test]
        public void PrewarmedResourcesAreLentBeforeNewOnes()
        {
            var pool = Pool(prewarm: 2, maxIdle: 4);
            pool.Prewarm();

            var first = pool.Resource(Borrower());
            var second = pool.Resource(Borrower());
            var third = pool.Resource(Borrower());

            Assert.That(_created, Has.Count.EqualTo(3));
            Assert.That(_created.GetRange(0, 2), Is.EquivalentTo(new[] { first, second }));
            Assert.That(third, Is.SameAs(_created[2]));
        }

        [Test]
        public void PrewarmTopsUpOnlyWhatIsMissing()
        {
            var pool = Pool(prewarm: 3, maxIdle: 4);
            var borrower = _lifetime.Lifetime.CreateNested();
            pool.Resource(borrower.Lifetime);
            borrower.Terminate();

            pool.Prewarm();
            pool.Prewarm();

            Assert.That(_created, Has.Count.EqualTo(3));
        }

        [Test]
        public void PrewarmDoesNotCountResourcesInUse()
        {
            var pool = Pool(prewarm: 2, maxIdle: 4);
            pool.Resource(Borrower());

            pool.Prewarm();

            Assert.That(_created, Has.Count.EqualTo(3));
        }

        [Test]
        public void PrewarmOfAnEndedPoolThrows()
        {
            var pool = Pool(prewarm: 1, maxIdle: 1);
            _poolLifetime.Terminate();

            Assert.Throws<LifetimeCanceledException>(() => pool.Prewarm());
        }

        [Test]
        public void UncappedPoolLendsAsManyAsAsked()
        {
            var pool = Pool(cap: PoolCap.None);

            for (var index = 0; index < 100; index++)
                pool.Resource(Borrower());

            Assert.That(_created, Has.Count.EqualTo(100));
            Assert.That(_created.TrueForAll(item => item.Borrow.IsAlive), Is.True);
        }

        [Test]
        public void ReclaimingCapEndsTheOldestBorrowToLendAnother()
        {
            var pool = Pool(cap: PoolCap.ReclaimOldest(2));
            var oldest = pool.Resource(Borrower());
            var oldestBorrow = oldest.Borrow;
            var middle = pool.Resource(Borrower());

            var newest = pool.Resource(Borrower());

            Assert.That(oldestBorrow.IsAlive, Is.False);
            Assert.That(middle.Borrow.IsAlive, Is.True);
            Assert.That(newest.Borrow.IsAlive, Is.True);
        }

        [Test]
        public void ReclaimedResourceGoesToTheNewBorrower()
        {
            var pool = Pool(cap: PoolCap.ReclaimOldest(1));
            var first = pool.Resource(Borrower());

            var second = pool.Resource(Borrower());

            Assert.That(second, Is.SameAs(first));
            Assert.That(_created, Has.Count.EqualTo(1));
        }

        [Test]
        public void ReclaimingCapSkipsBorrowsThatAlreadyEnded()
        {
            var pool = Pool(cap: PoolCap.ReclaimOldest(2));
            var returned = _lifetime.Lifetime.CreateNested();
            pool.Resource(returned.Lifetime);
            var kept = pool.Resource(Borrower());
            returned.Terminate();

            var newest = pool.Resource(Borrower());

            Assert.That(kept.Borrow.IsAlive, Is.True);
            Assert.That(newest.Borrow.IsAlive, Is.True);
        }

        [Test]
        public void ReclaimingCapTakesBorrowsInTheOrderTheyWereMade()
        {
            var pool = Pool(cap: PoolCap.ReclaimOldest(2));
            var first = pool.Resource(Borrower()).Borrow;
            var second = pool.Resource(Borrower()).Borrow;

            pool.Resource(Borrower());
            var secondAfterOne = second.IsAlive;
            pool.Resource(Borrower());

            Assert.That(first.IsAlive, Is.False);
            Assert.That(secondAfterOne, Is.True);
            Assert.That(second.IsAlive, Is.False);
        }

        [Test]
        public void ReclaimedResourceIsDestroyedWhenNothingMayIdle()
        {
            var pool = Pool(maxIdle: 0, cap: PoolCap.ReclaimOldest(1));
            var first = pool.Resource(Borrower());

            var second = pool.Resource(Borrower());

            Assert.That(first.Life.IsAlive, Is.False);
            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(second.Borrow.IsAlive, Is.True);
        }

        [Test]
        public void ThrowingCapRefusesABorrowOverTheMax()
        {
            var pool = Pool(cap: PoolCap.Throwing(2));
            var first = pool.Resource(Borrower());
            var second = pool.Resource(Borrower());

            Assert.Throws<InvalidOperationException>(() => pool.Resource(Borrower()));
            Assert.That(_created, Has.Count.EqualTo(2));
            Assert.That(first.Borrow.IsAlive, Is.True);
            Assert.That(second.Borrow.IsAlive, Is.True);
        }

        [Test]
        public void ThrowingCapLendsAgainAfterAReturn()
        {
            var pool = Pool(cap: PoolCap.Throwing(1));
            var borrower = _lifetime.Lifetime.CreateNested();
            var first = pool.Resource(borrower.Lifetime);
            borrower.Terminate();

            var second = pool.Resource(Borrower());

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void ThrowingCapLimitsBorrowsNotFreeResources()
        {
            var pool = Pool(prewarm: 3, maxIdle: 3, cap: PoolCap.Throwing(1));
            pool.Prewarm();

            pool.Resource(Borrower());

            Assert.Throws<InvalidOperationException>(() => pool.Resource(Borrower()));
        }

        private LimitedPool<Item> Pool(int prewarm = 0, int maxIdle = 8, PoolCap cap = null) =>
            LimitedPool<Item>.Create(_poolLifetime.Lifetime, Create, new PoolSettings(prewarm, maxIdle, cap ?? PoolCap.None));

        private Lifetime Borrower() =>
            _lifetime.Lifetime.CreateNested().Lifetime;

        private IResource<Item> Create(Lifetime life)
        {
            var item = new Item(life);
            _created.Add(item);
            return item;
        }

        private sealed class Item : IResource<Item>
        {
            public Lifetime Life { get; }
            public Lifetime Borrow { get; private set; } = Lifetime.Terminated;

            public Item(Lifetime life)
            {
                Life = life;
            }

            public Item Facade(Lifetime lifetime)
            {
                Borrow = lifetime;
                return this;
            }
        }
    }
}
