using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Variables;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class TickedStateFormTests
    {
        private readonly TickedStateForm<int, short> _form = new(new Halves());

        [Test]
        public void WireKeepsTheTickAndTakesTheStatesWireForm()
        {
            var wire = _form.WireOf(new TickedState<int>(120, 40));

            Assert.That(wire.Tick, Is.EqualTo(120));
            Assert.That(wire.State, Is.EqualTo((short)20));
        }

        [Test]
        public void ValueKeepsTheTickAndTakesTheStatesValue()
        {
            var value = _form.ValueOf(new TickedState<short>(-5, 7));

            Assert.That(value.Tick, Is.EqualTo(-5));
            Assert.That(value.State, Is.EqualTo(14));
        }

        [Test]
        public void StateSurvivesTheWire()
        {
            var state = new TickedState<int>(int.MaxValue, 64);

            var back = _form.ValueOf(_form.WireOf(state));

            Assert.That(back.Tick, Is.EqualTo(state.Tick));
            Assert.That(back.State, Is.EqualTo(state.State));
        }

        private sealed class Halves : IWireForm<int, short>
        {
            public short WireOf(int value) =>
                (short)(value / 2);

            public int ValueOf(short wire) =>
                wire * 2;
        }
    }
}
