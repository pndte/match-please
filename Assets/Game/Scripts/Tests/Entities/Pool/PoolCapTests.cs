using System;
using Bw.Entities.Pool;
using NUnit.Framework;

namespace Bw.Tests.Entities.Pool
{
    public sealed class PoolCapTests
    {
        [Test]
        public void NoneTakesTheUncappedBranch()
        {
            Assert.That(Branch(PoolCap.None), Is.EqualTo("none"));
        }

        [Test]
        public void NoneIsOneSharedValue()
        {
            Assert.That(PoolCap.None, Is.SameAs(PoolCap.None));
        }

        [TestCase(1)]
        [TestCase(48)]
        [TestCase(int.MaxValue)]
        public void ReclaimOldestTakesItsBranchWithItsMax(int max)
        {
            Assert.That(Branch(PoolCap.ReclaimOldest(max)), Is.EqualTo($"reclaim {max}"));
        }

        [TestCase(1)]
        [TestCase(16)]
        [TestCase(int.MaxValue)]
        public void ThrowingTakesItsBranchWithItsMax(int max)
        {
            Assert.That(Branch(PoolCap.Throwing(max)), Is.EqualTo($"throw {max}"));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void ReclaimingCapWithoutRoomIsRefused(int max)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PoolCap.ReclaimOldest(max));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void ThrowingCapWithoutRoomIsRefused(int max)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PoolCap.Throwing(max));
        }

        [Test]
        public void SwitchHandsTheStateToTheBranch()
        {
            var state = new object();
            object seen = null;

            PoolCap.Throwing(3).Switch(state, _ => { }, (_, _) => { }, (given, _) => seen = given);

            Assert.That(seen, Is.SameAs(state));
        }

        private static string Branch(PoolCap cap)
        {
            var taken = "";
            cap.Switch(0, _ => taken = "none", (_, max) => taken = $"reclaim {max}", (_, max) => taken = $"throw {max}");
            return taken;
        }
    }
}
