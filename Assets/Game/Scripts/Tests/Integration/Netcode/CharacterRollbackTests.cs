using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Level;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Physics;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.Integration.Netcode
{
    public sealed class CharacterRollbackTests
    {
        private const string MovementConfigPath = "Assets/Resources/Movement2DConfig.asset";
        private const int Delay = 4;
        private const int Ticks = 120;
        private const int ServerOnlyJumpTick = 50;

        private MovementConfig _config;
        private PhysicsTestWorld _server;
        private PhysicsTestWorld _client;
        private CharacterMovement _serverCharacter;
        private CharacterMovement _clientCharacter;
        private ManualResults<TickedState<MovementState>> _received;
        private Dictionary<int, MovementState> _serverStates;
        private Dictionary<int, MovementState> _clientStates;
        private List<int> _correctedAt;
        private int _clientSteps;
        private int _replayedSteps;
        private bool _jumpedOnlyOnServer;

        [SetUp]
        public void SetUp()
        {
            var rollbackConfig = new WorldRollbackConfig();
            _config = AssetDatabase.LoadAssetAtPath<MovementConfig>(MovementConfigPath);
            _server = new PhysicsTestWorld();
            _client = new PhysicsTestWorld();
            _serverCharacter = Character(_server);
            _clientCharacter = Character(_client);
            _received = new ManualResults<TickedState<MovementState>>();
            _serverStates = new Dictionary<int, MovementState>();
            _clientStates = new Dictionary<int, MovementState>();
            _correctedAt = new List<int>();
            _clientSteps = 0;
            _replayedSteps = 0;
            _jumpedOnlyOnServer = false;

            var serverStep = new WorldStep(_server.Lifetime, _server.Clock);
            new PhysicsSchedule(_server.Lifetime, _server.Space, serverStep);
            serverStep.Stepped(TickPhase.Default).Advise(_server.Lifetime, tick => _serverCharacter.Step(ServerControls(tick)));

            var rollback = new WorldRollback(_client.Lifetime, _client.Clock, rollbackConfig);
            new PhysicsSchedule(_client.Lifetime, _client.Space, rollback);
            rollback.Stepped(TickPhase.Default).Advise(_client.Lifetime, tick =>
            {
                _clientSteps++;
                _clientCharacter.Step(Controls(tick));
            });
            var replicated = new ReplicatedStates<MovementState>(_client.Lifetime, _received, rollbackConfig);
            rollback.Track(_client.Lifetime, new RollbackPart<MovementState>(_clientCharacter, replicated, new MovementDivergence(), rollbackConfig));
            new StateCorrections<MovementState>(_client.Lifetime, _clientCharacter, rollback).Corrected
                .Advise(_client.Lifetime, _ => _correctedAt.Add(_client.Clock.Current));
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _server.Dispose();
        }

        [Test]
        public void SameInputsKeepBothPeersInStepWithoutReplays()
        {
            Play();

            for (var tick = 1; tick <= Ticks; tick++)
                Assert.That(_clientStates[tick], Is.EqualTo(_serverStates[tick]), $"tick {tick}");

            Assert.That(_correctedAt, Is.Empty);
            Assert.That(_replayedSteps, Is.Zero);
        }

        [Test]
        public void CharacterRunsJumpsAndLandsOnTheFloor()
        {
            Play();

            var start = _serverStates[1].Body.Position;
            var highest = float.MinValue;
            for (var tick = 1; tick <= 30; tick++)
                highest = Mathf.Max(highest, _serverStates[tick].Body.Position.y);

            Assert.That(_serverStates[30].Body.Position.x - start.x, Is.GreaterThan(2f));
            Assert.That(highest - start.y, Is.GreaterThan(0.5f));
            Assert.That(_serverStates[Ticks].Body.Grounded, Is.True);
            Assert.That(_serverStates[Ticks].Body.Position.x, Is.LessThan(_serverStates[40].Body.Position.x));
        }

        [Test]
        public void DivergingServerStateIsCorrectedOnTheTickItArrives()
        {
            _jumpedOnlyOnServer = true;

            Play();

            Assert.That(_correctedAt, Is.Not.Empty);
            Assert.That(_correctedAt[0], Is.EqualTo(ServerOnlyJumpTick + Delay));
            for (var tick = 1; tick < ServerOnlyJumpTick; tick++)
                Assert.That(_clientStates[tick], Is.EqualTo(_serverStates[tick]), $"tick {tick}");
        }

        [Test]
        public void CorrectedCharacterAgreesWithTheServerOnceItLands()
        {
            _jumpedOnlyOnServer = true;

            Play();

            Assert.That(new MovementDivergence().Diverged(_clientStates[Ticks], _serverStates[Ticks]), Is.False);
            Assert.That(_clientStates[Ticks].Body.Grounded, Is.True);
        }

        private void Play()
        {
            var pending = new Queue<TickedState<MovementState>>();
            for (var tick = 1; tick <= Ticks; tick++)
            {
                _server.Clock.Tick(tick);
                _serverStates[tick] = _serverCharacter.State.Value;
                pending.Enqueue(new TickedState<MovementState>(tick, _serverCharacter.State.Value));
                while (pending.Count > 0 && pending.Peek().Tick <= tick - Delay)
                    _received.Deliver(pending.Dequeue());

                var stepsBefore = _clientSteps;
                _client.Clock.Tick(tick);
                _clientStates[tick] = _clientCharacter.State.Value;
                _replayedSteps += _clientSteps - stepsBefore - 1;
            }
        }

        private MovementInput ServerControls(int tick)
        {
            var controls = Controls(tick);
            if (!_jumpedOnlyOnServer || tick < ServerOnlyJumpTick || tick > ServerOnlyJumpTick + 6)
                return controls;

            return new MovementInput(controls.Horizontal, tick == ServerOnlyJumpTick, true, false, controls.Slowdown);
        }

        private static MovementInput Controls(int tick)
        {
            var horizontal = tick <= 30 ? 1f : tick > 40 && tick <= 70 ? -1f : 0f;
            return new MovementInput(horizontal, tick == 10, tick >= 10 && tick <= 18, false, Slowdown.None);
        }

        private CharacterMovement Character(PhysicsTestWorld world)
        {
            var floor = world.Box("Floor", new Vector2(0f, -0.5f), new Vector2(60f, 1f));
            LevelGeometry.Create(world.Lifetime, world.Space, new List<BodyBox> { floor });
            var box = world.Box("Character", new Vector2(0f, 1.05f), new Vector2(1f, 2f), Vector2.zero, PhysicsTestWorld.Character);
            var body = world.Space.CreateBody(world.Lifetime, box, PhysicsBody.BodyType.Kinematic);
            var shovables = new ShovableCollection();
            var rider = new PlatformRider(box, new MovingPlatformCollection(), shovables, world.Clock);
            var collider = new ShapeCollider(body, box, _config.GroundLayer, rider, shovables);
            var motor = new PlatformerMotor(new BodyMotor(collider, world.Clock, _config), world.Clock, _config);
            return new CharacterMovement(motor, new BodyPicture(box));
        }
    }
}
