using Bw.Entities.Network;
using Bw.Tests.Doubles;
using Bw.UseCases.Match.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Match.Network
{
    public sealed class LocalPlayerReceiverTests
    {
        private LifetimeDefinition _lifetime;
        private ManualResults<int> _told;
        private LocalPlayer _localPlayer;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _told = new ManualResults<int>();
            _localPlayer = new LocalPlayer();
            _ = new LocalPlayerReceiver(_lifetime.Lifetime, _told, _localPlayer);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void LocalPlayerIsUnknownUntilTheServerTells()
        {
            Assert.That(_localPlayer.Id.Value.IsKnown, Is.False);
        }

        [Test]
        public void ToldIdNamesTheLocalPlayer()
        {
            _told.Deliver(3);

            Assert.That(_localPlayer.Id.Value.Is(3), Is.True);
        }

        [Test]
        public void NewerIdReplacesTheOlderOne()
        {
            _told.Deliver(3);

            _told.Deliver(5);

            Assert.That(_localPlayer.Id.Value.Is(5), Is.True);
            Assert.That(_localPlayer.Id.Value.Is(3), Is.False);
        }

        [Test]
        public void LocalPlayerIsForgottenWithTheReceiver()
        {
            _told.Deliver(3);

            _lifetime.Terminate();

            Assert.That(_localPlayer.Id.Value.IsKnown, Is.False);
        }
    }
}
