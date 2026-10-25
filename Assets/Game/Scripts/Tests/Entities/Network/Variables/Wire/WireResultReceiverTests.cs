using System.Collections.Generic;
using System.Globalization;
using Bw.Entities.Network.Variables;
using Bw.Tests.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Variables
{
    public sealed class WireResultReceiverTests
    {
        private LifetimeDefinition _lifetime;
        private ManualResults<string> _wire;
        private WireResultReceiver<int, string> _receiver;
        private List<int> _received;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _wire = new ManualResults<string>();
            _receiver = new WireResultReceiver<int, string>(_lifetime.Lifetime, _wire, new DecimalForm());
            _received = new List<int>();
            _receiver.Received.Advise(_lifetime.Lifetime, _received.Add);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void ReceivedWireFormIsTurnedIntoTheValue()
        {
            _wire.Deliver("42");
            _wire.Deliver("-3");

            Assert.That(_received, Is.EqualTo(new[] { 42, -3 }));
        }

        [Test]
        public void EndedReceiverHearsNothing()
        {
            var receiver = new LifetimeDefinition();
            var heard = new List<int>();
            new WireResultReceiver<int, string>(receiver.Lifetime, _wire, new DecimalForm()).Received.Advise(_lifetime.Lifetime, heard.Add);
            receiver.Terminate();

            _wire.Deliver("42");

            Assert.That(heard, Is.Empty);
        }

        private sealed class DecimalForm : IWireForm<int, string>
        {
            public string WireOf(int value) =>
                value.ToString(CultureInfo.InvariantCulture);

            public int ValueOf(string wire) =>
                int.Parse(wire, CultureInfo.InvariantCulture);
        }
    }
}
