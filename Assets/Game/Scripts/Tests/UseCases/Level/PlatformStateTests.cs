using Bw.UseCases.Level;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Level
{
    public sealed class PlatformStateTests
    {
        [Test]
        public void StatesWithTheSameShareAreEqual()
        {
            Assert.That(new PlatformState(0.25f).Equals(new PlatformState(0.25f)), Is.True);
        }

        [Test]
        public void StatesWithDifferentSharesDiffer()
        {
            Assert.That(new PlatformState(0.25f).Equals(new PlatformState(0.2500001f)), Is.False);
        }

        [Test]
        public void DefaultStateIsAtTheStartOfTheRail()
        {
            Assert.That(default(PlatformState).Along, Is.Zero);
        }
    }
}
