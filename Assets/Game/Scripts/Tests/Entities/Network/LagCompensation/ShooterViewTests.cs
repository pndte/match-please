using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Tests.Entities.Network.LagCompensation.Doubles;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.LagCompensation
{
    public sealed class ShooterViewTests
    {
        private static readonly Client Shooter = new(3, new ConnectionRequest("Саня", 0));

        [Test]
        public void ViewKeepsBothTicks()
        {
            var view = new ShooterView(96.5, 104.25, PlayerDriver.Client(Shooter));

            Assert.That(view.Interpolated, Is.EqualTo(96.5));
            Assert.That(view.Predicted, Is.EqualTo(104.25));
        }

        [Test]
        public void ObjectInTheShootersBubbleIsSeenAtThePredictedTick()
        {
            var view = new ShooterView(96.5, 104.25, PlayerDriver.Client(Shooter));

            Assert.That(view.TickOf(new ScriptedBubble { Holds = true }), Is.EqualTo(104.25));
        }

        [Test]
        public void ObjectOutsideTheShootersBubbleIsSeenInterpolated()
        {
            var view = new ShooterView(96.5, 104.25, PlayerDriver.Client(Shooter));

            Assert.That(view.TickOf(new ScriptedBubble { Holds = false }), Is.EqualTo(96.5));
        }

        [Test]
        public void BubbleIsAskedAboutTheShootersClientAtThePredictedTick()
        {
            var bubble = new ScriptedBubble();

            new ShooterView(96.5, 104.25, PlayerDriver.Client(Shooter)).TickOf(bubble);

            Assert.That(bubble.Asked, Is.EqualTo(new[] { ((IClient)Shooter, 104.25) }));
        }

        [Test]
        public void ServerDrivenShooterSeesEverythingInterpolated()
        {
            var bubble = new ScriptedBubble { Holds = true };

            var tick = new ShooterView(96.5, 104.25, PlayerDriver.Server).TickOf(bubble);

            Assert.That(tick, Is.EqualTo(96.5));
            Assert.That(bubble.Asked, Is.Empty);
        }

        [TestCase(90.0, 100.0, 95.0, 105.0, 95.0, 100.0)]
        [TestCase(96.0, 99.0, 95.0, 105.0, 96.0, 99.0)]
        [TestCase(110.0, 120.0, 95.0, 105.0, 105.0, 105.0)]
        [TestCase(80.0, 85.0, 95.0, 105.0, 95.0, 95.0)]
        public void ClampedViewKeepsEachTickInsideTheRange(
            double interpolated,
            double predicted,
            double earliest,
            double latest,
            double clampedInterpolated,
            double clampedPredicted)
        {
            var clamped = new ShooterView(interpolated, predicted, PlayerDriver.Client(Shooter)).Clamped(earliest, latest);

            Assert.That(clamped.Interpolated, Is.EqualTo(clampedInterpolated));
            Assert.That(clamped.Predicted, Is.EqualTo(clampedPredicted));
        }

        [Test]
        public void ClampedViewStillJudgesByTheShootersBubble()
        {
            var clamped = new ShooterView(90.0, 110.0, PlayerDriver.Client(Shooter)).Clamped(95.0, 105.0);

            Assert.That(clamped.TickOf(new ScriptedBubble { Holds = true }), Is.EqualTo(105.0));
            Assert.That(clamped.TickOf(new ScriptedBubble { Holds = false }), Is.EqualTo(95.0));
        }
    }
}
