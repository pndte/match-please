using Bw.UseCases.Bots.Navigation;
using Bw.UseCases.Bots.Navigation.Extensions;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Bots.Navigation.Extensions
{
    public sealed class NavManeuverExtensionsTests
    {
        [Test]
        public void ManeuverWithoutJumpOrDropIsARun()
        {
            Assert.That(new NavManeuver { Horizontal = 1f }.IsRun(), Is.True);
        }

        [Test]
        public void JumpIsNoRun()
        {
            Assert.That(new NavManeuver { Horizontal = 1f, JumpTicks = 5 }.IsRun(), Is.False);
        }

        [Test]
        public void DropIsNoRun()
        {
            Assert.That(new NavManeuver { Dropping = true }.IsRun(), Is.False);
        }

        [TestCase(0, 0f)]
        [TestCase(2, 0f)]
        [TestCase(3, -1f)]
        [TestCase(40, -1f)]
        public void RunStartsAfterItsDelay(int tick, float horizontal)
        {
            var maneuver = new NavManeuver { Horizontal = -1f, RunDelay = 3 };

            Assert.That(maneuver.InputAt(tick).Horizontal, Is.EqualTo(horizontal));
        }

        [Test]
        public void JumpIsPressedOnlyOnTheFirstTick()
        {
            var maneuver = new NavManeuver { JumpTicks = 4 };

            Assert.That(maneuver.InputAt(0).JumpPressed, Is.True);
            Assert.That(maneuver.InputAt(1).JumpPressed, Is.False);
        }

        [TestCase(0, true)]
        [TestCase(3, true)]
        [TestCase(4, false)]
        public void JumpIsHeldForItsTicks(int tick, bool held)
        {
            var maneuver = new NavManeuver { JumpTicks = 4 };

            Assert.That(maneuver.InputAt(tick).JumpHeld, Is.EqualTo(held));
        }

        [Test]
        public void RunNeverJumps()
        {
            var maneuver = new NavManeuver { Horizontal = 1f };

            Assert.That(maneuver.InputAt(0).JumpPressed, Is.False);
            Assert.That(maneuver.InputAt(0).JumpHeld, Is.False);
        }

        [Test]
        public void DropHoldsDownEveryTick()
        {
            var maneuver = new NavManeuver { Dropping = true };

            Assert.That(maneuver.InputAt(0).Down, Is.True);
            Assert.That(maneuver.InputAt(10).Down, Is.True);
        }

        [Test]
        public void ManeuverIsNeverSlowedDown()
        {
            var input = new NavManeuver { Horizontal = 1f, JumpTicks = 3 }.InputAt(1);

            Assert.That(input.Slowdown.Speed, Is.Zero);
            Assert.That(input.Slowdown.JumpHeight, Is.Zero);
        }
    }
}
