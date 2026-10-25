using System;
using Bw.Entities.Network.Rollback;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Rollback
{
    public sealed class TickHistoryTests
    {
        [Test]
        public void EmptyHistoryHasNoNewestTick()
        {
            var history = new TickHistory<int>(4);

            Assert.That(history.NewestTick, Is.EqualTo(int.MinValue));
            Assert.That(history.TryGetAt(0, out _), Is.False);
            Assert.That(history.TryGetLatest(100, out _, out _), Is.False);
            Assert.That(history.TryGetEarliest(-100, out _, out _), Is.False);
        }

        [Test]
        public void RecordedValueIsReadAtItsTick()
        {
            var history = new TickHistory<int>(4);

            history.Record(7, 70);

            Assert.That(history.TryGetAt(7, out var value), Is.True);
            Assert.That(value, Is.EqualTo(70));
            Assert.That(history.NewestTick, Is.EqualTo(7));
        }

        [Test]
        public void TickWithoutRecordIsMissing()
        {
            var history = new TickHistory<int>(4);
            history.Record(7, 70);

            Assert.That(history.TryGetAt(6, out var value), Is.False);
            Assert.That(value, Is.EqualTo(default(int)));
        }

        [Test]
        public void RecordingTheSameTickAgainReplacesTheValue()
        {
            var history = new TickHistory<int>(4);
            history.Record(7, 70);

            history.Record(7, 71);

            Assert.That(history.TryGetAt(7, out var value), Is.True);
            Assert.That(value, Is.EqualTo(71));
        }

        [Test]
        public void HistoryKeepsOnlyTheNewestCapacityTicks()
        {
            var history = new TickHistory<int>(4);
            for (var tick = 1; tick <= 6; tick++)
                history.Record(tick, tick * 10);

            Assert.That(history.TryGetAt(2, out _), Is.False);
            Assert.That(history.TryGetAt(3, out var oldest), Is.True);
            Assert.That(oldest, Is.EqualTo(30));
            Assert.That(history.TryGetAt(6, out var newest), Is.True);
            Assert.That(newest, Is.EqualTo(60));
        }

        [Test]
        public void SlotOverwrittenByANewerTickDoesNotAnswerForTheOldOne()
        {
            var history = new TickHistory<int>(4);
            history.Record(1, 10);

            history.Record(5, 50);

            Assert.That(history.TryGetAt(1, out _), Is.False);
            Assert.That(history.TryGetAt(5, out var value), Is.True);
            Assert.That(value, Is.EqualTo(50));
        }

        [Test]
        public void OlderTickInsideTheWindowIsRecordedWithoutMovingTheNewest()
        {
            var history = new TickHistory<int>(4);
            history.Record(10, 100);

            history.Record(8, 80);

            Assert.That(history.NewestTick, Is.EqualTo(10));
            Assert.That(history.TryGetAt(8, out var value), Is.True);
            Assert.That(value, Is.EqualTo(80));
        }

        [Test]
        public void TickOlderThanTheWindowThrows()
        {
            var history = new TickHistory<int>(4);
            history.Record(10, 100);

            Assert.Throws<ArgumentOutOfRangeException>(() => history.Record(6, 60));
        }

        [Test]
        public void EmptySlotMarkerIsNeverRecorded()
        {
            var history = new TickHistory<int>(4);

            Assert.Throws<ArgumentOutOfRangeException>(() => history.Record(int.MinValue, 1));
        }

        [Test]
        public void NegativeTicksAreKeptInTheirOwnSlots()
        {
            var history = new TickHistory<int>(4);
            for (var tick = -5; tick <= -2; tick++)
                history.Record(tick, tick);

            for (var tick = -5; tick <= -2; tick++)
            {
                Assert.That(history.TryGetAt(tick, out var value), Is.True);
                Assert.That(value, Is.EqualTo(tick));
            }
        }

        [Test]
        public void LatestFindsTheNewestRecordNotAfterTheTick()
        {
            var history = new TickHistory<int>(8);
            history.Record(2, 20);
            history.Record(5, 50);
            history.Record(7, 70);

            Assert.That(history.TryGetLatest(6, out var tick, out var value), Is.True);
            Assert.That(tick, Is.EqualTo(5));
            Assert.That(value, Is.EqualTo(50));
        }

        [Test]
        public void LatestAfterTheNewestTickAnswersWithTheNewest()
        {
            var history = new TickHistory<int>(8);
            history.Record(2, 20);
            history.Record(5, 50);

            Assert.That(history.TryGetLatest(1000, out var tick, out var value), Is.True);
            Assert.That(tick, Is.EqualTo(5));
            Assert.That(value, Is.EqualTo(50));
        }

        [Test]
        public void LatestBeforeEveryRecordIsMissing()
        {
            var history = new TickHistory<int>(8);
            history.Record(5, 50);

            Assert.That(history.TryGetLatest(4, out _, out _), Is.False);
        }

        [Test]
        public void EarliestFindsTheOldestRecordNotBeforeTheTick()
        {
            var history = new TickHistory<int>(8);
            history.Record(2, 20);
            history.Record(5, 50);
            history.Record(7, 70);

            Assert.That(history.TryGetEarliest(3, out var tick, out var value), Is.True);
            Assert.That(tick, Is.EqualTo(5));
            Assert.That(value, Is.EqualTo(50));
        }

        [Test]
        public void EarliestBeforeTheWindowStartsAtTheWindow()
        {
            var history = new TickHistory<int>(4);
            for (var tick = 1; tick <= 6; tick++)
                history.Record(tick, tick * 10);

            Assert.That(history.TryGetEarliest(int.MinValue + 1, out var found, out var value), Is.True);
            Assert.That(found, Is.EqualTo(3));
            Assert.That(value, Is.EqualTo(30));
        }

        [Test]
        public void EarliestAfterTheNewestTickIsMissing()
        {
            var history = new TickHistory<int>(4);
            history.Record(5, 50);

            Assert.That(history.TryGetEarliest(6, out _, out _), Is.False);
        }

        [Test]
        public void ClearForgetsEveryRecord()
        {
            var history = new TickHistory<int>(4);
            history.Record(1, 10);
            history.Record(2, 20);

            history.Clear();

            Assert.That(history.NewestTick, Is.EqualTo(int.MinValue));
            Assert.That(history.TryGetAt(1, out _), Is.False);
            Assert.That(history.TryGetAt(2, out _), Is.False);
        }

        [Test]
        public void ClearedHistoryAcceptsAnyTickAgain()
        {
            var history = new TickHistory<int>(4);
            history.Record(100, 1);
            history.Clear();

            history.Record(3, 30);

            Assert.That(history.NewestTick, Is.EqualTo(3));
            Assert.That(history.TryGetAt(3, out var value), Is.True);
            Assert.That(value, Is.EqualTo(30));
        }

        [Test]
        public void CapacityIsTheSizeItWasMadeWith()
        {
            Assert.That(new TickHistory<int>(31).Capacity, Is.EqualTo(31));
        }
    }
}
