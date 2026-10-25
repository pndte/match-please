using Bw.Entities.Network.Prediction.Events;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Events
{
    public sealed class ActionIdTests
    {
        [Test]
        public void ActionKeepsItsInitiatorAndTick()
        {
            var action = new ActionId(42UL, 1200);

            Assert.That(action.Initiator, Is.EqualTo(42UL));
            Assert.That(action.Tick, Is.EqualTo(1200));
        }

        [TestCase(7UL, 100, true)]
        [TestCase(8UL, 100, false)]
        [TestCase(7UL, 101, false)]
        [TestCase(8UL, 101, false)]
        public void ActionsAreEqualWhenBothInitiatorAndTickMatch(ulong initiator, int tick, bool equal)
        {
            var action = new ActionId(7UL, 100);

            Assert.That(action.Equals(new ActionId(initiator, tick)), Is.EqualTo(equal));
        }

        [Test]
        public void DefaultActionIsInitiatorZeroAtTickZero()
        {
            Assert.That(default(ActionId).Equals(new ActionId(0UL, 0)), Is.True);
        }
    }
}
