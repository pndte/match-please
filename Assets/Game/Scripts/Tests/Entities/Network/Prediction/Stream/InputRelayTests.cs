using System.Collections.Generic;
using System.Linq;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using Bw.UseCases;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class InputRelayTests
    {
        private LifetimeDefinition _lifetime;
        private DriverInputs<int> _inputs;
        private ScriptedWorldStep _worldStep;
        private RecordingResults<TickedInput<int>> _relayed;
        private Audience _audience;
        private IClient _first;
        private IClient _second;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            var buffer = new PredictionInputBuffer<int>(
                _lifetime.Lifetime,
                new ManualClock(),
                new NetworkTicksConfig(),
                new AnyInput(),
                new SameInput(),
                new ManualResults<TickedInput<int>>());
            _inputs = new DriverInputs<int>(_lifetime.Lifetime, new ControllerRegistry(), new PlayerDrivers(), new ControllerSamplers<int>(), buffer);
            _worldStep = new ScriptedWorldStep();
            _relayed = new RecordingResults<TickedInput<int>>();
            _audience = new Audience();
            _first = new Client(default);
            _second = new Client(default);
            new InputRelay<int>(_lifetime.Lifetime, _inputs, _worldStep, _relayed, _audience);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void HeardInputsReachEveryClientOfTheAudienceAtPublish()
        {
            _audience.Clients.AddRange(new[] { _first, _second });
            _inputs.TryTakeInputAt(5, out _);
            _inputs.TryTakeInputAt(6, out _);

            _worldStep.Fire(TickPhase.Publish, 6);

            Assert.That(_relayed.Sent.Select(sent => (sent.Recipient, sent.Result.Tick)), Is.EqualTo(new[] { (_first, 5), (_second, 5), (_first, 6), (_second, 6) }));
            Assert.That(_relayed.Broadcasts, Is.Empty);
        }

        [TestCase(TickPhase.Prepare)]
        [TestCase(TickPhase.Early)]
        [TestCase(TickPhase.Default)]
        [TestCase(TickPhase.Late)]
        public void HeardInputsWaitForPublish(TickPhase phase)
        {
            _audience.Clients.Add(_first);
            _inputs.TryTakeInputAt(5, out _);

            _worldStep.Fire(phase, 5);

            Assert.That(_relayed.Sent, Is.Empty);
        }

        [Test]
        public void TickWithNothingHeardAsksNoAudience()
        {
            _worldStep.Fire(TickPhase.Publish, 5);

            Assert.That(_audience.Asked, Is.Zero);
        }

        [Test]
        public void InputIsRelayedOnce()
        {
            _audience.Clients.Add(_first);
            _inputs.TryTakeInputAt(5, out _);
            _worldStep.Fire(TickPhase.Publish, 5);

            _worldStep.Fire(TickPhase.Publish, 6);

            Assert.That(_relayed.Sent, Has.Count.EqualTo(1));
        }

        [Test]
        public void InputNobodyNeededIsDropped()
        {
            _inputs.TryTakeInputAt(5, out _);
            _worldStep.Fire(TickPhase.Publish, 5);
            _audience.Clients.Add(_first);

            _worldStep.Fire(TickPhase.Publish, 6);

            Assert.That(_relayed.Sent, Is.Empty);
        }

        [Test]
        public void AudienceIsAskedAgainEveryTick()
        {
            _audience.Clients.Add(_first);
            _inputs.TryTakeInputAt(5, out _);
            _worldStep.Fire(TickPhase.Publish, 5);
            _audience.Clients.Clear();
            _audience.Clients.Add(_second);

            _inputs.TryTakeInputAt(6, out _);
            _worldStep.Fire(TickPhase.Publish, 6);

            Assert.That(_relayed.Sent.Select(sent => sent.Recipient), Is.EqualTo(new[] { _first, _second }));
            Assert.That(_audience.Asked, Is.EqualTo(2));
        }

        [Test]
        public void RelayCarriesTheInputAndItsPrevious()
        {
            _audience.Clients.Add(_first);
            _inputs.TryTakeInputAt(5, out _);

            _worldStep.Fire(TickPhase.Publish, 5);

            Assert.That(_relayed.Sent[0].Result, Is.EqualTo(new TickedInput<int>(5, 0, 0)));
        }

        private sealed class Audience : IInputAudience
        {
            public List<IClient> Clients { get; } = new();
            public int Asked { get; private set; }

            public void Collect(List<IClient> clients)
            {
                Asked++;
                clients.AddRange(Clients);
            }
        }

        private sealed class AnyInput : IInputPolicy<int>
        {
            public bool IsValid(int input) =>
                true;

            public int CaughtUp(
                int input,
                int lastInput,
                int heard,
                int heardTicksAgo) =>
                input;
        }

        private sealed class SameInput : IInputExtrapolator<int>
        {
            public int Continued(int last, int ticks) =>
                last;
        }
    }
}
