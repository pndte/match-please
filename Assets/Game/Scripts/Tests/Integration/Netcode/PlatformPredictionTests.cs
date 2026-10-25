using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Rollback;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.Tests.UseCases.Level.Doubles;
using Bw.UseCases.Level;
using Bw.UseCases.Level.Network;
using Bw.UseCases.Level.Network.Codecs;
using JetBrains.Collections.Viewable;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Tests.Integration.Netcode
{
    public sealed class PlatformPredictionTests
    {
        private const int Delay = 4;
        private const int Ticks = 330;

        private static readonly MovingPlatformPath Path = new() { Travel = new Vector2(0f, 22.5f), TravelSeconds = 4f, PauseSeconds = 1f };

        private PhysicsTestWorld _server;
        private PhysicsTestWorld _client;
        private Lift _serverLift;
        private Lift _clientLift;
        private ManualResults<TickedState<PlatformState>> _received;
        private List<int> _correctedAt;
        private List<float> _gaps;
        private float _lowest;
        private float _highest;

        [SetUp]
        public void SetUp()
        {
            var config = new WorldRollbackConfig();
            _server = new PhysicsTestWorld();
            _client = new PhysicsTestWorld();
            _serverLift = new Lift(_server, Path.Travel);
            _clientLift = new Lift(_client, Path.Travel);
            _received = new ManualResults<TickedState<PlatformState>>();
            _correctedAt = new List<int>();
            _gaps = new List<float>();
            _lowest = float.MaxValue;
            _highest = float.MinValue;

            var serverStep = new WorldStep(_server.Lifetime, _server.Clock);
            new PhysicsSchedule(_server.Lifetime, _server.Space, serverStep);
            new MovingPlatformSchedule(_server.Lifetime, _serverLift.Platform, Path, _server.Clock, serverStep);

            var predicted = new Predicted();
            var rollback = new WorldRollback(_client.Lifetime, _client.Clock, config);
            new PhysicsSchedule(_client.Lifetime, _client.Space, rollback);
            var replicated = new ReplicatedStates<PlatformState>(_client.Lifetime, _received, config);
            var part = new RollbackPart<PlatformState>(_clientLift.Platform, replicated, new PlatformDivergence(_clientLift.Rail), config);
            new RollbackPartRegistration<PlatformState>(_client.Lifetime, predicted, part, rollback);
            new PredictedPlatformSchedule(_client.Lifetime, _clientLift.Platform, Path, _client.Clock, predicted, rollback);
            new StateCorrections<PlatformState>(_client.Lifetime, _clientLift.Platform, rollback).Corrected
                .Advise(_client.Lifetime, _ => _correctedAt.Add(_client.Clock.Current));
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _server.Dispose();
        }

        [Test]
        public void PredictedLiftMovesWhereTheServerMovesItWithoutCorrections()
        {
            Play();

            Assert.That(_correctedAt, Is.Empty);
            Assert.That(_gaps, Has.Count.EqualTo(Ticks));
            Assert.That(_gaps, Has.All.LessThan(0.001f));
        }

        [Test]
        public void LiftTravelsTheWholeShaftAndBackDuringTheRun()
        {
            Play();

            Assert.That(_lowest, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(_highest, Is.EqualTo(1f).Within(1e-4f));
        }

        private void Play()
        {
            var pending = new Queue<TickedState<PlatformState>>();
            for (var tick = 1; tick <= Ticks; tick++)
            {
                _server.Clock.Tick(tick);
                var server = _serverLift.Platform.State.Value.Along;
                _lowest = Mathf.Min(_lowest, server);
                _highest = Mathf.Max(_highest, server);
                pending.Enqueue(Wire(new TickedState<PlatformState>(tick, _serverLift.Platform.State.Value)));
                while (pending.Count > 0 && pending.Peek().Tick <= tick - Delay)
                    _received.Deliver(pending.Dequeue());

                _client.Clock.Tick(tick);
                _gaps.Add(Mathf.Abs(_clientLift.Platform.State.Value.Along - server) * Path.Travel.magnitude);
            }
        }

        private static TickedState<PlatformState> Wire(TickedState<PlatformState> state)
        {
            using var writer = new FastBufferWriter(16, Allocator.Temp);
            writer.WriteNetworkSerializable(new TickedPlatformStateCodec { Value = state });
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out TickedPlatformStateCodec decoded);
            return decoded.Value;
        }

        private sealed class Predicted : IPredictedBy
        {
            public IReadonlyProperty<bool> Me { get; } = new ViewableProperty<bool>(true);
        }
    }
}
