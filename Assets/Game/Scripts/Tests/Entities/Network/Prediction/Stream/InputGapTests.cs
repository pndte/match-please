using Bw.Entities.Network.Prediction.Stream;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class InputGapTests
    {
        [Test]
        public void LateGapKnowsHowLateTheInputWas()
        {
            var gap = InputGap.Late(3);

            Assert.That(gap.Kind, Is.EqualTo(InputGapKind.Late));
            Assert.That(gap.LateTicks, Is.EqualTo(3));
        }

        [Test]
        public void OtherGapsCarryNoLateness()
        {
            Assert.That((InputGap.Early.Kind, InputGap.Early.LateTicks), Is.EqualTo((InputGapKind.Early, 0)));
            Assert.That((InputGap.Recovered.Kind, InputGap.Recovered.LateTicks), Is.EqualTo((InputGapKind.Recovered, 0)));
            Assert.That((InputGap.Substituted.Kind, InputGap.Substituted.LateTicks), Is.EqualTo((InputGapKind.Substituted, 0)));
        }
    }
}
