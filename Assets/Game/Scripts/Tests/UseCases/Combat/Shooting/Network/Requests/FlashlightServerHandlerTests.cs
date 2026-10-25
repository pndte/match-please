using Bw.Tests.Doubles;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Shooting.Network.Requests;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Shooting.Network.Requests
{
    public sealed class FlashlightServerHandlerTests
    {
        private LifetimeDefinition _lifetime;
        private ManualResults<bool> _requests;
        private Flashlight _flashlight;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _requests = new ManualResults<bool>();
            _flashlight = new Flashlight();
            new FlashlightServerHandler(_lifetime.Lifetime, _requests, _flashlight);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void RequestedStateIsApplied()
        {
            _requests.Deliver(true);

            Assert.That(_flashlight.Lit.Value, Is.True);
        }

        [Test]
        public void RepeatedRequestKeepsTheState()
        {
            _requests.Deliver(true);

            _requests.Deliver(true);

            Assert.That(_flashlight.Lit.Value, Is.True);
        }

        [Test]
        public void RequestToSwitchOffIsApplied()
        {
            _requests.Deliver(true);

            _requests.Deliver(false);

            Assert.That(_flashlight.Lit.Value, Is.False);
        }

        [Test]
        public void RequestsAfterTheHandlersLifetimeAreIgnored()
        {
            _lifetime.Terminate();

            _requests.Deliver(true);

            Assert.That(_flashlight.Lit.Value, Is.False);
        }
    }
}
