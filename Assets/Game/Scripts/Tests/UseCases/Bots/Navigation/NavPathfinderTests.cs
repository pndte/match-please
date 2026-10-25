using System;
using System.Collections.Generic;
using System.Linq;
using Bw.Tests.UseCases.Bots.Navigation.Doubles;
using Bw.UseCases.Bots.Navigation;
using NUnit.Framework;
using UnityEngine;
using static Bw.Tests.UseCases.Bots.Navigation.Doubles.Graphs;

namespace Bw.Tests.UseCases.Bots.Navigation
{
    public sealed class NavPathfinderTests
    {
        private Graphs _graphs;
        private List<NavEdge> _path;

        [SetUp]
        public void SetUp()
        {
            _graphs = new Graphs();
            _path = new List<NavEdge>();
        }

        [TearDown]
        public void TearDown() =>
            _graphs.Dispose();

        [Test]
        public void PathToTheSameNodeIsEmpty()
        {
            var pathfinder = new NavPathfinder(_graphs.Graph(Row(2), Edge(0, 1, 3)));

            Assert.That(pathfinder.TryFind(1, 1, _path), Is.True);
            Assert.That(_path, Is.Empty);
        }

        [Test]
        public void PathFollowsTheEdgesInOrder()
        {
            var pathfinder = new NavPathfinder(_graphs.Graph(Row(3), Edge(0, 1, 3), Edge(1, 2, 4)));

            Assert.That(pathfinder.TryFind(0, 2, _path), Is.True);
            Assert.That(_path.Select(edge => (edge.From, edge.To)), Is.EqualTo(new[] { (0, 1), (1, 2) }));
        }

        [Test]
        public void CheaperDetourBeatsTheDirectEdge()
        {
            var pathfinder = new NavPathfinder(_graphs.Graph(Row(3), Edge(0, 2, 10), Edge(0, 1, 2), Edge(1, 2, 3)));

            pathfinder.TryFind(0, 2, _path);

            Assert.That(_path.Select(edge => edge.To), Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void CheaperOfTwoParallelEdgesIsTaken()
        {
            var pathfinder = new NavPathfinder(_graphs.Graph(Row(2), Edge(0, 1, 9), Edge(0, 1, 4)));

            pathfinder.TryFind(0, 1, _path);

            Assert.That(_path.Single().Ticks, Is.EqualTo(4));
        }

        [Test]
        public void UnreachableNodeHasNoPath()
        {
            var pathfinder = new NavPathfinder(_graphs.Graph(Row(3), Edge(0, 1, 3)));
            _path.Add(Edge(9, 9, 9));

            Assert.That(pathfinder.TryFind(0, 2, _path), Is.False);
            Assert.That(_path, Is.Empty);
        }

        [Test]
        public void EdgesLeadOneWay()
        {
            var pathfinder = new NavPathfinder(_graphs.Graph(Row(2), Edge(0, 1, 3)));

            Assert.That(pathfinder.TryFind(0, 1, _path), Is.True);
            Assert.That(pathfinder.TryFind(1, 0, _path), Is.False);
        }

        [Test]
        public void SearchesOneAfterAnotherDoNotLeakIntoEachOther()
        {
            var pathfinder = new NavPathfinder(_graphs.Graph(Row(4), Edge(0, 1, 1), Edge(1, 2, 1), Edge(2, 3, 1), Edge(3, 0, 1)));

            pathfinder.TryFind(0, 3, _path);
            pathfinder.TryFind(2, 1, _path);

            Assert.That(_path.Select(edge => edge.To), Is.EqualTo(new[] { 3, 0, 1 }));
        }

        [Test]
        public void ClimbWithoutHorizontalMoveIsStillFound()
        {
            var nodes = new[] { Vector2.zero, new Vector2(0f, 3f), new Vector2(0f, 6f) };
            var pathfinder = new NavPathfinder(_graphs.Graph(nodes, Edge(0, 1, 20), Edge(1, 2, 20)));

            Assert.That(pathfinder.TryFind(0, 2, _path), Is.True);
            Assert.That(_path, Has.Count.EqualTo(2));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void PathIsAsCheapAsTheCheapestPossible(int seed)
        {
            var random = new System.Random(seed);
            var nodes = Enumerable.Range(0, 30).Select(_ => new Vector2(random.Next(0, 40), random.Next(0, 10))).ToArray();
            var edges = new List<NavEdge>();
            for (var index = 0; index < 120; index++)
            {
                var from = random.Next(nodes.Length);
                var to = random.Next(nodes.Length);
                if (from == to)
                    continue;

                var fastest = Mathf.CeilToInt(Mathf.Abs(nodes[to].x - nodes[from].x) / 4f);
                edges.Add(Edge(from, to, Math.Max(1, fastest + random.Next(0, 20))));
            }

            var graph = _graphs.Graph(nodes, edges.ToArray());
            var pathfinder = new NavPathfinder(graph);
            for (var target = 1; target < nodes.Length; target++)
            {
                var cheapest = Cheapest(edges, nodes.Length, 0, target);
                var found = pathfinder.TryFind(0, target, _path);

                Assert.That(found, Is.EqualTo(cheapest < int.MaxValue), $"target {target}");
                if (!found)
                    continue;

                Assert.That(_path.Sum(edge => edge.Ticks), Is.EqualTo(cheapest), $"target {target}");
                Assert.That(_path[0].From, Is.Zero);
                Assert.That(_path[^1].To, Is.EqualTo(target));
                for (var step = 1; step < _path.Count; step++)
                    Assert.That(_path[step].From, Is.EqualTo(_path[step - 1].To));
            }
        }

        private static int Cheapest(
            IReadOnlyList<NavEdge> edges,
            int count,
            int from,
            int to)
        {
            var costs = Enumerable.Repeat(int.MaxValue, count).ToArray();
            costs[from] = 0;
            for (var round = 0; round < count; round++)
                foreach (var edge in edges)
                    if (costs[edge.From] != int.MaxValue && costs[edge.From] + edge.Ticks < costs[edge.To])
                        costs[edge.To] = costs[edge.From] + edge.Ticks;

            return costs[to];
        }
    }
}
