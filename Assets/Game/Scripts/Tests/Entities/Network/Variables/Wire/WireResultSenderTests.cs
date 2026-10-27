using System.Globalization;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Tests.Entities.Network.Doubles;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Variables
{
    public sealed class WireResultSenderTests
    {
        [Test]
        public void BroadcastSendsTheWireForm()
        {
            var wire = new RecordingResults<string>();
            var sender = new WireResultSender<int, string>(wire, new DecimalForm());

            sender.Broadcast(42);

            Assert.That(wire.Broadcasts, Is.EqualTo(new[] { "42" }));
            Assert.That(wire.Sent, Is.Empty);
        }

        [Test]
        public void SendToSendsTheWireFormToItsRecipient()
        {
            var wire = new RecordingResults<string>();
            var sender = new WireResultSender<int, string>(wire, new DecimalForm());
            var recipient = new Client(new ConnectionRequest("Сева", 4, System.Guid.NewGuid(), System.Guid.Empty));

            sender.SendTo(recipient, -3);

            Assert.That(wire.Sent, Has.Count.EqualTo(1));
            Assert.That(wire.Sent[0].Recipient, Is.SameAs(recipient));
            Assert.That(wire.Sent[0].Value, Is.EqualTo("-3"));
            Assert.That(wire.Broadcasts, Is.Empty);
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
