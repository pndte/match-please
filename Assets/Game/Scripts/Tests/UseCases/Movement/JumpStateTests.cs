using Bw.UseCases.Movement;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Movement
{
    public sealed class JumpStateTests
    {
        [Test]
        public void StatesWithTheSameFieldsAreEqual()
        {
            Assert.That(new JumpState(true, 3, 2).Equals(new JumpState(true, 3, 2)), Is.True);
        }

        [TestCase(false, 3, 2)]
        [TestCase(true, 2, 2)]
        [TestCase(true, 3, 0)]
        public void AnyFieldMakesADifference(bool rising, int coyoteTicks, int bufferedTicks)
        {
            Assert.That(new JumpState(true, 3, 2).Equals(new JumpState(rising, coyoteTicks, bufferedTicks)), Is.False);
        }
    }
}
