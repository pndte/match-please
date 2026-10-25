using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Players;
using Bw.Tests.Doubles;
using Bw.UseCases;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class DriverInputsTests
    {
        private LifetimeDefinition _lifetime;
        private ControllerRegistry _controllers;
        private PlayerDrivers _drivers;
        private ControllerSamplers<int> _samplers;
        private ManualResults<TickedInput<int>> _requests;
        private DriverInputs<int> _inputs;
        private List<TickedInput<int>> _heard;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _controllers = new ControllerRegistry();
            _drivers = new PlayerDrivers();
            _samplers = new ControllerSamplers<int>();
            _requests = new ManualResults<TickedInput<int>>();
            var buffer = new PredictionInputBuffer<int>(
                _lifetime.Lifetime,
                new ManualClock(),
                new NetworkTicksConfig(),
                new AnyInput(),
                new SameInput(),
                _requests);
            _inputs = new DriverInputs<int>(_lifetime.Lifetime, _controllers, _drivers, _samplers, buffer);
            _heard = new List<TickedInput<int>>();
            _inputs.Heard.Advise(_lifetime.Lifetime, _heard.Add);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void ObjectNobodyControlsStepsWithTheDefaultInput()
        {
            var taken = _inputs.TryTakeInputAt(5, out var input);

            Assert.That(taken, Is.True);
            Assert.That(input, Is.Zero);
            Assert.That(_heard, Is.EqualTo(new[] { new TickedInput<int>(5, 0, 0) }));
        }

        [Test]
        public void ClientDrivenObjectTakesTheClientsBufferedInput()
        {
            Control(ClientPlayer());
            Hear(5, 50, 49);

            var taken = _inputs.TryTakeInputAt(5, out var input);

            Assert.That(taken, Is.True);
            Assert.That(input, Is.EqualTo(50));
        }

        [Test]
        public void ClientsInputIsHeardWhenItIsStoredAndNotAgainWhenTaken()
        {
            Control(ClientPlayer());

            Hear(5, 50, 49);
            _inputs.TryTakeInputAt(5, out _);

            Assert.That(_heard, Is.EqualTo(new[] { new TickedInput<int>(5, 50, 49) }));
        }

        [Test]
        public void ClientDrivenObjectWaitsForTheFirstInput()
        {
            Control(ClientPlayer());

            Assert.That(_inputs.TryTakeInputAt(5, out _), Is.False);
            Assert.That(_heard, Is.Empty);
        }

        [Test]
        public void ServerDrivenObjectSamplesItsController()
        {
            var bot = ServerPlayer(new SequenceSampler(7, 8));
            Control(bot);

            _inputs.TryTakeInputAt(5, out var first);
            _inputs.TryTakeInputAt(6, out var second);

            Assert.That((first, second), Is.EqualTo((7, 8)));
            Assert.That(_heard, Is.EqualTo(new[] { new TickedInput<int>(5, 7, 0), new TickedInput<int>(6, 8, 7) }));
        }

        [Test]
        public void ServerDrivenObjectIgnoresTheNetwork()
        {
            Control(ServerPlayer(new SequenceSampler(7)));

            Hear(5, 50, 49);
            _inputs.TryTakeInputAt(5, out var input);

            Assert.That(input, Is.EqualTo(7));
            Assert.That(_heard, Has.Count.EqualTo(1));
        }

        [Test]
        public void ServerDrivenControllerWithoutASamplerThrows()
        {
            var bot = new Player("bot");
            _drivers.AddServerDriven(_lifetime.Lifetime, bot);
            Control(bot);

            Assert.Throws<InvalidOperationException>(() => _inputs.TryTakeInputAt(5, out _));
        }

        [Test]
        public void ServerDrivenObjectTakesOneController()
        {
            Control(ServerPlayer(new SequenceSampler(1)));
            LogAssert.Expect(LogType.Exception, new Regex("second one came"));

            Control(ServerPlayer(new SequenceSampler(2)));

            _inputs.TryTakeInputAt(5, out var input);
            Assert.That(input, Is.EqualTo(1));
        }

        [Test]
        public void ControllerLeavingStopsAnnouncingItsInputs()
        {
            var control = new LifetimeDefinition();
            Control(ClientPlayer(), control.Lifetime);

            control.Terminate();
            Hear(5, 50, 49);

            Assert.That(_heard, Is.Empty);
        }

        [Test]
        public void NewControllerStartsWithAnEmptyBuffer()
        {
            var first = new LifetimeDefinition();
            Control(ClientPlayer(), first.Lifetime);
            Hear(5, 50, 49);
            first.Terminate();

            Control(ClientPlayer());

            Assert.That(_inputs.TryTakeInputAt(5, out _), Is.False);
        }

        [Test]
        public void ObjectLeftByItsServerDrivenControllerStepsWithTheDefaultInput()
        {
            var control = new LifetimeDefinition();
            Control(ServerPlayer(new SequenceSampler(7)), control.Lifetime);
            _inputs.TryTakeInputAt(5, out _);

            control.Terminate();
            _inputs.TryTakeInputAt(6, out var input);

            Assert.That(input, Is.Zero);
            Assert.That(_heard[^1], Is.EqualTo(new TickedInput<int>(6, 0, 7)));
        }

        [Test]
        public void ServerDrivenControllerCanBeFollowedByAnother()
        {
            var first = new LifetimeDefinition();
            Control(ServerPlayer(new SequenceSampler(7)), first.Lifetime);
            first.Terminate();

            Control(ServerPlayer(new SequenceSampler(9)));
            _inputs.TryTakeInputAt(5, out var input);

            Assert.That(input, Is.EqualTo(9));
        }

        private IPlayer ClientPlayer()
        {
            var player = new Player("client");
            _drivers.AddClientDriven(_lifetime.Lifetime, player, new Client(1UL, default));
            return player;
        }

        private IPlayer ServerPlayer(IInputSampler<int> sampler)
        {
            var player = new Player("bot");
            _drivers.AddServerDriven(_lifetime.Lifetime, player);
            _samplers.Add(player, sampler);
            return player;
        }

        private void Control(IPlayer player) =>
            Control(player, _lifetime.Lifetime);

        private void Control(IPlayer player, Lifetime lifetime) =>
            _controllers.Set(lifetime, player);

        private void Hear(int tick, int input, int previous) =>
            _requests.Deliver(new TickedInput<int>(tick, input, previous));

        private sealed class SequenceSampler : IInputSampler<int>
        {
            private readonly int[] _samples;
            private int _next;

            public SequenceSampler(params int[] samples)
            {
                _samples = samples;
            }

            public int Sample() =>
                _samples[Math.Min(_next++, _samples.Length - 1)];
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

        private sealed class SameInput : IInputContinuation<int>
        {
            public int Continued(int last, int ticks) =>
                last;
        }
    }
}
