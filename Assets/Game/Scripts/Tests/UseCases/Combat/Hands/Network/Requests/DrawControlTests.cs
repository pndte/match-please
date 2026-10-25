using Bw.UseCases.Combat.Hands.Network.Requests;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Hands.Network.Requests
{
    public sealed class DrawControlTests
    {
        [Test]
        public void KeepAndLastCarryNoValue()
        {
            Assert.That(DrawControl.Keep.Kind, Is.EqualTo(DrawControlKind.Keep));
            Assert.That(DrawControl.Keep.Value, Is.Zero);
            Assert.That(DrawControl.Last.Kind, Is.EqualTo(DrawControlKind.Last));
            Assert.That(DrawControl.Last.Value, Is.Zero);
        }

        [TestCase(0)]
        [TestCase(2)]
        public void SlotControlCarriesTheSlotIndex(int index)
        {
            var control = DrawControl.ToSlot(index);

            Assert.That(control.Kind, Is.EqualTo(DrawControlKind.Slot));
            Assert.That(control.Value, Is.EqualTo(index));
        }

        [TestCase(1)]
        [TestCase(-1)]
        public void ScrollControlCarriesTheDirection(int direction)
        {
            var control = DrawControl.Scrolled(direction);

            Assert.That(control.Kind, Is.EqualTo(DrawControlKind.Scroll));
            Assert.That(control.Value, Is.EqualTo(direction));
        }

        [Test]
        public void ControlsOfOneKindAndValueAreEqual()
        {
            Assert.That(DrawControl.ToSlot(1).Equals(DrawControl.ToSlot(1)), Is.True);
            Assert.That(DrawControl.ToSlot(1).Equals((object)DrawControl.ToSlot(1)), Is.True);
            Assert.That(DrawControl.ToSlot(1).GetHashCode(), Is.EqualTo(DrawControl.ToSlot(1).GetHashCode()));
        }

        [Test]
        public void ControlsOfAnotherKindOrValueDiffer()
        {
            Assert.That(DrawControl.ToSlot(1).Equals(DrawControl.ToSlot(2)), Is.False);
            Assert.That(DrawControl.ToSlot(1).Equals(DrawControl.Scrolled(1)), Is.False);
            Assert.That(DrawControl.Keep.Equals(DrawControl.Last), Is.False);
        }
    }
}
