using Bw.UseCases.Combat.Hands.Network;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Hands.Network
{
    public sealed class TriggerLatchTests
    {
        [Test]
        public void NewLatchPullsExactlyWhileTheButtonIsHeld()
        {
            var latch = new TriggerLatch();

            Assert.That(latch.Latched, Is.False);
            Assert.That(latch.Pulled(true), Is.True);
            Assert.That(latch.Pulled(false), Is.False);
        }

        [Test]
        public void ButtonHeldWhenControlBeginsIsNoTrigger()
        {
            var latch = new TriggerLatch();

            latch.Begin(true);

            Assert.That(latch.Pulled(true), Is.False);
        }

        [Test]
        public void ButtonReleasedWhenControlBeginsPullsOnceHeld()
        {
            var latch = new TriggerLatch();

            latch.Begin(false);

            Assert.That(latch.Pulled(true), Is.True);
        }

        [Test]
        public void HoldCarriedOverStaysIgnoredUntilAPressIsLatched()
        {
            var latch = new TriggerLatch();
            latch.Begin(true);

            var released = latch.Pulled(false);
            var heldAgain = latch.Pulled(true);

            Assert.That(released, Is.False);
            Assert.That(heldAgain, Is.False);
        }

        [Test]
        public void LatchedPressPullsEvenAfterAQuickRelease()
        {
            var latch = new TriggerLatch();

            latch.Latch();

            Assert.That(latch.Latched, Is.True);
            Assert.That(latch.Pulled(false), Is.True);
        }

        [Test]
        public void LatchedPressEndsAHoldCarriedOver()
        {
            var latch = new TriggerLatch();
            latch.Begin(true);

            latch.Latch();
            latch.Clear();

            Assert.That(latch.Pulled(true), Is.True);
        }

        [Test]
        public void ClearForgetsTheLatchedPress()
        {
            var latch = new TriggerLatch();
            latch.Latch();

            latch.Clear();

            Assert.That(latch.Latched, Is.False);
            Assert.That(latch.Pulled(false), Is.False);
        }

        [Test]
        public void ClearKeepsAHoldCarriedOver()
        {
            var latch = new TriggerLatch();
            latch.Begin(true);

            latch.Clear();

            Assert.That(latch.Pulled(true), Is.False);
        }

        [Test]
        public void BeginForgetsALatchedPress()
        {
            var latch = new TriggerLatch();
            latch.Latch();

            latch.Begin(false);

            Assert.That(latch.Latched, Is.False);
            Assert.That(latch.Pulled(false), Is.False);
        }
    }
}
