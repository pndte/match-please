using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class InputMarginClientHandlerTests
    {
        private LifetimeDefinition _lifetime;
        private Toggle _controlled;
        private ManualResults<int> _margins;
        private Feedback _feedback;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _controlled = new Toggle();
            _margins = new ManualResults<int>();
            _feedback = new Feedback();
            new InputMarginClientHandler(_lifetime.Lifetime, _controlled, _margins, _feedback);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void MarginOfAnObjectControlledHereReachesTheFeedback()
        {
            _controlled.Set(true);

            _margins.Deliver(2);
            _margins.Deliver(-1);

            Assert.That(_feedback.Reports, Is.EqualTo(new[] { 2, -1 }));
        }

        [Test]
        public void MarginOfAnObjectControlledElsewhereIsIgnored()
        {
            _margins.Deliver(2);

            Assert.That(_feedback.Reports, Is.Empty);
        }

        [Test]
        public void LosingControlStopsTheFeedback()
        {
            _controlled.Set(true);
            _margins.Deliver(2);

            _controlled.Set(false);
            _margins.Deliver(3);

            Assert.That(_feedback.Reports, Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void RegainedControlFeedsAgain()
        {
            _controlled.Set(true);
            _controlled.Set(false);

            _controlled.Set(true);
            _margins.Deliver(4);

            Assert.That(_feedback.Reports, Is.EqualTo(new[] { 4 }));
        }

        private sealed class Feedback : IInputMarginFeedback
        {
            public List<int> Reports { get; } = new();

            public void Report(int marginTicks) =>
                Reports.Add(marginTicks);
        }
    }
}
