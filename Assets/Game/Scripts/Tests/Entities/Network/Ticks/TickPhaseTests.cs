using System;
using Bw.Entities.Network.Ticks;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Ticks
{
    public sealed class TickPhaseTests
    {
        [Test]
        public void PhasesRunInTheOrderOfATick()
        {
            Assert.That(Enum.GetValues(typeof(TickPhase)), Is.EqualTo(new[]
            {
                TickPhase.Prepare,
                TickPhase.Early,
                TickPhase.Physics,
                TickPhase.Default,
                TickPhase.Late,
                TickPhase.Publish
            }));
        }

        [Test]
        public void PhasesAreNumberedFromZeroWithoutGaps()
        {
            var phases = (TickPhase[])Enum.GetValues(typeof(TickPhase));

            for (var index = 0; index < phases.Length; index++)
                Assert.That((int)phases[index], Is.EqualTo(index));
        }
    }
}
