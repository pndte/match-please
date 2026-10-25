using System;
using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.Codecs;
using Bw.Tests.Entities.Network.Doubles;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network
{
    public sealed class SendersCollectionTests
    {
        private MessageSender<int, IntCodec> _ints;
        private MessageSender<bool, BoolCodec> _bools;
        private SendersCollection _senders;

        [SetUp]
        public void SetUp()
        {
            var router = new RecordingRouter();
            _ints = new MessageSender<int, IntCodec>(router, router);
            _bools = new MessageSender<bool, BoolCodec>(router, router);
            _senders = new SendersCollection(new Dictionary<Type, IMessageSender>
            {
                [typeof(int)] = _ints,
                [typeof(bool)] = _bools
            });
        }

        [Test]
        public void ClientGetsTheSenderOfThePayloadType()
        {
            IClientSendersCollection senders = _senders;

            Assert.That(senders.Get<int>(), Is.SameAs(_ints));
            Assert.That(senders.Get<bool>(), Is.SameAs(_bools));
        }

        [Test]
        public void ServerGetsTheSameSenders()
        {
            IServerSendersCollection senders = _senders;

            Assert.That(senders.Get<int>(), Is.SameAs(_ints));
            Assert.That(senders.Get<bool>(), Is.SameAs(_bools));
        }

        [Test]
        public void TypeWithoutASenderThrows()
        {
            IClientSendersCollection senders = _senders;

            Assert.Throws<KeyNotFoundException>(() => senders.Get<string>());
        }

        [Test]
        public void ReceiversAreLookedUpByTheirPayloadType()
        {
            var byType = new Dictionary<Type, IMessageReceiver> { [typeof(int)] = new MessageReceiver<int, IntCodec>() };

            var receivers = new MessageReceivers(byType);

            Assert.That(receivers.ByType[typeof(int)], Is.TypeOf<MessageReceiver<int, IntCodec>>());
            Assert.That(receivers.ByType.ContainsKey(typeof(bool)), Is.False);
        }
    }
}
