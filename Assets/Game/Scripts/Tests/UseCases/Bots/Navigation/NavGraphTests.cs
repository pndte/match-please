using System;
using System.Linq;
using Bw.Tests.UseCases.Bots.Navigation.Doubles;
using Bw.UseCases.Bots.Navigation;
using NUnit.Framework;
using UnityEngine;
using static Bw.Tests.UseCases.Bots.Navigation.Doubles.Graphs;

namespace Bw.Tests.UseCases.Bots.Navigation
{
    public sealed class NavGraphTests
    {
        private Graphs _graphs;

        [SetUp]
        public void SetUp() =>
            _graphs = new Graphs();

        [TearDown]
        public void TearDown() =>
            _graphs.Dispose();

        [Test]
        public void GraphWithoutNodesIsRefused()
        {
            Assert.Throws<InvalidOperationException>(() => _graphs.Graph(Array.Empty<Vector2>()));
        }

        [TestCase(-1, 1, 5)]
        [TestCase(0, 3, 5)]
        [TestCase(3, 0, 5)]
        [TestCase(0, -1, 5)]
        [TestCase(0, 1, 0)]
        [TestCase(0, 1, -2)]
        public void BrokenEdgeIsRefused(int from, int to, int ticks)
        {
            Assert.Throws<InvalidOperationException>(() => _graphs.Graph(Row(3), Edge(from, to, ticks)));
        }

        [Test]
        public void NodesKeepTheirPositions()
        {
            var graph = _graphs.Graph(new[] { new Vector2(1f, 2f), new Vector2(-3f, 4f) });

            Assert.That(graph.NodeCount, Is.EqualTo(2));
            Assert.That(graph.NodeAt(1), Is.EqualTo(new Vector2(-3f, 4f)));
        }

        [Test]
        public void EdgesOfANodeAreFoundTogetherCheapestFirst()
        {
            var graph = _graphs.Graph(Row(3), Edge(1, 2, 9), Edge(0, 1, 5), Edge(1, 0, 3), Edge(0, 2, 2));

            var first = Enumerable.Range(graph.FirstEdgeOf(0), graph.EdgeEndOf(0) - graph.FirstEdgeOf(0)).Select(graph.EdgeAt).ToArray();
            var second = Enumerable.Range(graph.FirstEdgeOf(1), graph.EdgeEndOf(1) - graph.FirstEdgeOf(1)).Select(graph.EdgeAt).ToArray();

            Assert.That(first.Select(edge => (edge.To, edge.Ticks)), Is.EqualTo(new[] { (2, 2), (1, 5) }));
            Assert.That(second.Select(edge => (edge.To, edge.Ticks)), Is.EqualTo(new[] { (0, 3), (2, 9) }));
        }

        [Test]
        public void NodeWithoutEdgesHasAnEmptyRange()
        {
            var graph = _graphs.Graph(Row(3), Edge(0, 1, 5));

            Assert.That(graph.EdgeEndOf(2), Is.EqualTo(graph.FirstEdgeOf(2)));
            Assert.That(graph.EdgeEndOf(1), Is.EqualTo(graph.FirstEdgeOf(1)));
        }

        [Test]
        public void MaxSpeedIsTheFastestHorizontalEdge()
        {
            var graph = _graphs.Graph(Row(4, 2f), Edge(0, 1, 4), Edge(1, 3, 2), Edge(3, 0, 12));

            Assert.That(graph.MaxSpeed, Is.EqualTo(2f));
        }

        [Test]
        public void VerticalEdgesGiveNoSpeed()
        {
            var graph = _graphs.Graph(new[] { Vector2.zero, new Vector2(0f, 5f) }, Edge(0, 1, 10));

            Assert.That(graph.MaxSpeed, Is.Zero);
        }

        [Test]
        public void BakedAssetIsLeftAsItWas()
        {
            var asset = _graphs.Asset(Row(3), Edge(2, 1, 4), Edge(0, 1, 5));

            NavGraph.Of(asset);

            Assert.That(asset.Edges[0].From, Is.EqualTo(2));
            Assert.That(asset.Edges[1].From, Is.EqualTo(0));
        }

        [Test]
        public void StandingNodeIsTheClosestNodeUnderTheFeet()
        {
            var graph = _graphs.Graph(new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.4f, 3f) });

            Assert.That(graph.TryFindStandingNode(new Vector2(0.7f, 0.2f), out var node), Is.True);
            Assert.That(node, Is.EqualTo(1));
        }

        [TestCase(0.5f, 0.31f)]
        [TestCase(0.76f, 0f)]
        [TestCase(-0.76f, 0f)]
        public void NoNodeCloseEnoughUnderTheFeetIsNoStandingNode(float x, float y)
        {
            var graph = _graphs.Graph(new[] { Vector2.zero });

            Assert.That(graph.TryFindStandingNode(new Vector2(x, y), out var node), Is.False);
            Assert.That(node, Is.EqualTo(-1));
        }

        [Test]
        public void NodeBelowIsTheNearestNodeNotAboveTheFeet()
        {
            var graph = _graphs.Graph(new[] { new Vector2(0f, 0f), new Vector2(3f, 0f), new Vector2(0f, 4f) });

            Assert.That(graph.TryFindNodeBelow(new Vector2(0.3f, 1f), out var onCrate), Is.True);
            Assert.That(onCrate, Is.Zero);
            Assert.That(graph.TryFindNodeBelow(new Vector2(1f, 4.5f), out var upstairs), Is.True);
            Assert.That(upstairs, Is.EqualTo(2));
        }

        [Test]
        public void NodeAboveTheFeetIsNoNodeBelow()
        {
            var graph = _graphs.Graph(new[] { new Vector2(0f, 5f) });

            Assert.That(graph.TryFindNodeBelow(Vector2.zero, out var node), Is.False);
            Assert.That(node, Is.EqualTo(-1));
        }

        [Test]
        public void NearestNodeIsFoundInAnyDirection()
        {
            var graph = _graphs.Graph(new[] { new Vector2(0f, 0f), new Vector2(10f, 0f), new Vector2(0f, 4f) });

            Assert.That(graph.NearestNode(new Vector2(1f, 3f)), Is.EqualTo(2));
            Assert.That(graph.NearestNode(new Vector2(100f, -50f)), Is.EqualTo(1));
        }

        [Test]
        public void NearestOfEqualNodesIsTheFirst()
        {
            var graph = _graphs.Graph(new[] { new Vector2(-1f, 0f), new Vector2(1f, 0f) });

            Assert.That(graph.NearestNode(Vector2.zero), Is.Zero);
        }
    }
}
