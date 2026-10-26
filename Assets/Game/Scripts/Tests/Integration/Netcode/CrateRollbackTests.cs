using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Physics;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.UseCases.Level;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Network.Prediction;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.Integration.Netcode
{
    public sealed class CrateRollbackTests
    {
        private const string CrateConfig = "Assets/Game/Content/Crates/CrateBodyConfig.asset";
        private const int CrateLayer = 16;
        private const int Delay = 4;
        private const int Ticks = 120;
        private const int PushTicks = 45;
        private const int UnknownImpulseTick = 20;

        private PhysicsTestWorld _server;
        private PhysicsTestWorld _client;
        private DynamicBody _serverCrate;
        private DynamicBody _clientCrate;
        private RollbackStepper _stepper;
        private ManualResults<TickedState<RigidState>> _received;
        private Dictionary<int, RigidState> _serverStates;
        private Dictionary<int, RigidState> _clientStates;
        private Dictionary<int, int> _replayedBy;
        private List<int> _correctedAt;
        private int _steps;

        [SetUp]
        public void SetUp()
        {
            var config = new WorldRollbackConfig();
            _server = new PhysicsTestWorld();
            _client = new PhysicsTestWorld();
            _serverCrate = Crate(_server);
            _clientCrate = Crate(_client);
            _received = new ManualResults<TickedState<RigidState>>();
            _serverStates = new Dictionary<int, RigidState>();
            _clientStates = new Dictionary<int, RigidState>();
            _replayedBy = new Dictionary<int, int>();
            _correctedAt = new List<int>();
            _steps = 0;

            var serverStep = new WorldStep(_server.Lifetime, _server.Clock);
            new PhysicsStepper(_server.Lifetime, _server.Space, serverStep);
            serverStep.Stepped(TickPhase.Default).Advise(_server.Lifetime, ShoveTheServerCrate);

            _stepper = new RollbackStepper(_client.Lifetime, _client.Clock, config);
            new PhysicsStepper(_client.Lifetime, _client.Space, _stepper);
            _stepper.Stepped(TickPhase.Default).Advise(_client.Lifetime, ShoveTheClientCrate);
            var replicated = new ReplicatedStates<RigidState>(_client.Lifetime, _received, config);
            _stepper.Track(_client.Lifetime, new RollbackPart<RigidState>(_clientCrate, replicated, new RigidDivergence(), config));
            new StateCorrections<RigidState>(_client.Lifetime, _clientCrate, _stepper).Corrected.Advise(_client.Lifetime, _ => _correctedAt.Add(_client.Clock.Current));
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _server.Dispose();
        }

        [Test]
        public void IdenticalWorldsAgreeWithoutReplaysUntilTheUnknownImpulseArrives()
        {
            Play();

            for (var tick = 1; tick <= UnknownImpulseTick; tick++)
                Assert.That(Error(tick), Is.Zero, $"tick {tick}");

            for (var tick = 1; tick <= UnknownImpulseTick + Delay; tick++)
                Assert.That(_replayedBy[tick], Is.Zero, $"tick {tick}");
        }

        [Test]
        public void UnknownServerImpulseIsReplayedOnTheTickItsStateArrives()
        {
            Play();

            Assert.That(_correctedAt, Is.Not.Empty);
            Assert.That(_correctedAt[0], Is.EqualTo(UnknownImpulseTick + 1 + Delay));
            Assert.That(_replayedBy[UnknownImpulseTick + 1 + Delay], Is.EqualTo(Delay - 1));
        }

        [Test]
        public void ClientCrateEndsWhereTheServerCrateRests()
        {
            Play();

            Assert.That(Error(Ticks - Delay - 1), Is.LessThan(0.01f));
            Assert.That(Error(Ticks), Is.LessThan(0.01f));
            Assert.That(_serverStates[Ticks].Position.y, Is.InRange(-0.1f, 0.6f));
        }

        [Test]
        public void ReplaysNeverReachBeyondTheReplayWindow()
        {
            Play();

            foreach (var replayed in _replayedBy.Values)
                Assert.That(replayed, Is.LessThanOrEqualTo(new WorldRollbackConfig().MaxReplayTicks));
        }

        private void Play()
        {
            var pending = new Queue<TickedState<RigidState>>();
            for (var tick = 1; tick <= Ticks; tick++)
            {
                _server.Clock.Tick(tick);
                _serverStates[tick] = _serverCrate.State.Value;
                pending.Enqueue(new TickedState<RigidState>(tick, _serverCrate.State.Value));
                while (pending.Count > 0 && pending.Peek().Tick <= tick - Delay)
                    _received.Deliver(pending.Dequeue());

                var stepsBefore = _steps;
                _client.Clock.Tick(tick);
                _clientStates[tick] = _clientCrate.State.Value;
                _replayedBy[tick] = _steps - stepsBefore - 1;
            }
        }

        private void ShoveTheServerCrate(int tick)
        {
            _serverCrate.Shove(Push(tick));
            if (tick == UnknownImpulseTick)
                _serverCrate.Shove(new Vector2(0f, 900f));
        }

        private void ShoveTheClientCrate(int tick)
        {
            _steps++;
            _clientCrate.Shove(Push(tick));
        }

        private float Error(int tick) =>
            (_serverStates[tick].Position - _clientStates[tick].Position).magnitude;

        private static Vector2 Push(int tick) =>
            tick < PushTicks ? new Vector2(60f, 0f) : Vector2.zero;

        private static DynamicBody Crate(PhysicsTestWorld world)
        {
            var floor = world.Box("Floor", new Vector2(0f, -0.5f), new Vector2(60f, 1f));
            var wall = world.Box("Wall", new Vector2(9f, 2f), new Vector2(1f, 5f));
            LevelGeometry.Create(world.Lifetime, world.Space, new List<BodyBox> { floor, wall });
            var box = world.Box("Crate", Vector2.zero, Vector2.one, new Vector2(0f, 0.5f), CrateLayer);
            var config = AssetDatabase.LoadAssetAtPath<DynamicBodyConfig>(CrateConfig);
            var body = config.CreateBody(world.Space, world.Lifetime, box, PhysicsBody.BodyType.Dynamic);
            return new DynamicBody(world.Lifetime, body, world.Space);
        }
    }
}
