using System;
using Bw.Entities.Pool;
using Bw.Entities.Pool.GameObjects.Modes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Pool.GameObjects.Modes
{
    public sealed class PoolCapModeTests
    {
        [Test]
        public void ReclaimOldestModeMakesAReclaimingCapWithItsMax()
        {
            var mode = new ReclaimOldestMode { Max = 48 };

            Assert.That(Branch(mode.Cap()), Is.EqualTo("reclaim 48"));
        }

        [Test]
        public void ThrowingModeMakesAThrowingCapWithItsMax()
        {
            var mode = new ThrowingMode { Max = 3 };

            Assert.That(Branch(mode.Cap()), Is.EqualTo("throw 3"));
        }

        [Test]
        public void UncappedModeMakesNoCap()
        {
            Assert.That(new UncappedMode().Cap(), Is.SameAs(PoolCap.None));
        }

        [Test]
        public void ReclaimOldestModeWithoutRoomIsRefusedWhenItMakesItsCap()
        {
            var mode = new ReclaimOldestMode { Max = 0 };

            Assert.Throws<ArgumentOutOfRangeException>(() => mode.Cap());
        }

        [Test]
        public void ThrowingModeWithoutRoomIsRefusedWhenItMakesItsCap()
        {
            var mode = new ThrowingMode { Max = 0 };

            Assert.Throws<ArgumentOutOfRangeException>(() => mode.Cap());
        }

        [Test]
        public void ModeMakesAFreshCapFromItsCurrentMax()
        {
            var mode = new ReclaimOldestMode { Max = 2 };
            var before = mode.Cap();

            mode.Max = 5;

            Assert.That(Branch(before), Is.EqualTo("reclaim 2"));
            Assert.That(Branch(mode.Cap()), Is.EqualTo("reclaim 5"));
        }

        private static string Branch(PoolCap cap)
        {
            var taken = "";
            cap.Switch(0, _ => taken = "none", (_, max) => taken = $"reclaim {max}", (_, max) => taken = $"throw {max}");
            return taken;
        }
    }
}
