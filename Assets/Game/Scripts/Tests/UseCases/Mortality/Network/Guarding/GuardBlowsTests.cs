using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.Network.Guarding;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality.Network.Guarding
{
    public sealed class GuardBlowsTests
    {
        [Test]
        public void EveryBlowGoesToTheGuardWhole()
        {
            var guard = new AbsorbedBlows();
            var blows = new GuardBlows(guard);
            var impacts = Blows.Of(Blows.At(1f, 0f, 10f), Blows.At(-1f, 0f, 20f));

            blows.Affect(StateCause.Of(new ActionId(3UL, 50)), impacts);

            Assert.That(guard.Absorbed, Has.Count.EqualTo(1));
            Assert.That(guard.Absorbed[0].Count, Is.EqualTo(2));
            Assert.That(guard.Absorbed[0][1].Hit.Damage, Is.EqualTo(20f));
        }

        [Test]
        public void BlowWithoutCauseGoesToTheGuardToo()
        {
            var guard = new AbsorbedBlows();
            var blows = new GuardBlows(guard);

            blows.Affect(StateCause.None, Blows.Of(Blows.At(1f, 0f, 10f)));

            Assert.That(guard.Absorbed, Has.Count.EqualTo(1));
        }

        private sealed class AbsorbedBlows : IGuardBlows
        {
            public List<Impacts> Absorbed { get; } = new();

            public void Absorb(Impacts impacts) =>
                Absorbed.Add(impacts);
        }
    }
}
