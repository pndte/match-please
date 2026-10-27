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
    public sealed class BotNavigatorTests
    {
        private const string MovementConfigPath = "Assets/Game/Content/Movement2DConfig.asset";

        private Doubles.Graphs _graphs;
        private FakeMovement _movement;
        private BotNavigator _navigator;

        [SetUp]
        public void SetUp()
        {
            _graphs = new Doubles.Graphs();
            var graph = _graphs.Graph(Row(5), Run(0, 1), Run(1, 2), Run(2, 3), Run(3, 4));
            _movement = new FakeMovement();
            var config = AssetDatabase.LoadAssetAtPath<MovementConfig>(MovementConfigPath);
            var follower = new PathFollower(graph, _movement, new BotGamepad(), config, new Step());
            _navigator = new BotNavigator(graph, new NavPathfinder(graph), follower, _movement);
        }

        [TearDown]
        public void TearDown() =>
            _graphs.Dispose();

        [Test]
        public void BotOnTheFloorHeadsToTheDestination()
        {
            _movement.MoveTo(new Vector2(0.2f, 0f));

            Assert.That(_navigator.TryGoTo(new Vector2(4f, 0f)), Is.True);
            Assert.That(_navigator.Status, Is.EqualTo(NavStatus.Moving));
        }

        [Test]
        public void BotStandingOnACrateHeadsFromTheFloorUnderIt()
        {
            _movement.MoveTo(new Vector2(0.2f, 1f));

            Assert.That(_navigator.TryGoTo(new Vector2(4f, 0f)), Is.True, "crates, lifts and knockback leave a bot above the baked floor, and a bot that finds no way from there stands still for good");
            Assert.That(_navigator.Status, Is.EqualTo(NavStatus.Moving));
        }

        [Test]
        public void BotUnderEveryNodeFindsNoWay()
        {
            _movement.MoveTo(new Vector2(2f, -5f));

            Assert.That(_navigator.TryGoTo(new Vector2(4f, 0f)), Is.False);
        }

        private static NavEdge Run(int from, int to)
        {
            var edge = Edge(from, to, 3);
            edge.Maneuver = new NavManeuver { Horizontal = 1f };
            return edge;
        }

        private sealed class Step : ISimulationStep
        {
            public float Duration => 1f / 30f;
        }
    }
}
