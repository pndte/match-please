using Bw.Entities.Simulation;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Bots;
using Bw.UseCases.Bots.Navigation;
using Bw.UseCases.Movement;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using static Bw.Tests.UseCases.Bots.Navigation.Doubles.Graphs;

namespace Bw.Tests.UseCases.Bots.Navigation
{
    public sealed class PathFollowerTests
    {
        private const string MovementConfigPath = "Assets/Game/Content/Movement2DConfig.asset";
        private const float WireSteps = 8f;
        private const int Patience = 30;

        private Doubles.Graphs _graphs;
        private FakeMovement _movement;
        private BotGamepad _gamepad;
        private MovementConfig _config;
        private Step _step;
        private PathFollower _follower;
        private NavEdge _leap;

        [SetUp]
        public void SetUp()
        {
            _graphs = new Doubles.Graphs();
            _leap = Edge(0, 1, 18);
            _leap.Maneuver = new NavManeuver { Horizontal = 1f, JumpTicks = 10 };
            var graph = _graphs.Graph(new[] { Vector2.zero, new Vector2(3f, 0f) }, _leap);
            _movement = new FakeMovement();
            _gamepad = new BotGamepad();
            _config = AssetDatabase.LoadAssetAtPath<MovementConfig>(MovementConfigPath);
            _step = new Step();
            _follower = new PathFollower(graph, _movement, _gamepad, _config, _step);
        }

        [TearDown]
        public void TearDown() =>
            _graphs.Dispose();

        [TestCase(0.023f)]
        [TestCase(-0.04f)]
        [TestCase(0.3f)]
        public void BotOffTheLeapsStartLeapsThoughItsSteeringIsRoundedForTheWire(float offset)
        {
            _movement.MoveTo(new Vector2(offset, 0f));
            _follower.Follow(new[] { _leap });

            Assert.That(TicksToTheJump(), Is.LessThan(Patience), "a bot that walked to a node stops a few centimetres off it, and a steer finer than the wire carries never moves it");
        }

        private int TicksToTheJump()
        {
            for (var tick = 0; tick < Patience; tick++)
            {
                _gamepad.Release();
                _follower.Steer();
                if (_gamepad.Movement.JumpPressed)
                    return tick;

                var sent = Mathf.Round(_gamepad.Movement.Horizontal * WireSteps) / WireSteps;
                _movement.MoveTo(_movement.State.Value.Body.Position + Vector2.right * (sent * _config.Speed * _step.Duration));
            }

            return Patience;
        }

        private sealed class Step : ISimulationStep
        {
            public float Duration => 1f / 30f;
        }
    }
}
