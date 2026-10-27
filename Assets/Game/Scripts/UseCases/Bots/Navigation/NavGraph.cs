using System;
using UnityEngine;

namespace Bw.UseCases.Bots.Navigation
{
    public sealed class NavGraph
    {
        private const float StandingHeight = 0.3f;
        private const float StandingReach = 0.75f;

        public int NodeCount => _nodes.Length;
        public float MaxSpeed { get; }

        private readonly Vector2[] _nodes;
        private readonly NavEdge[] _edges;
        private readonly int[] _firstEdges;

        private NavGraph(
            Vector2[] nodes,
            NavEdge[] edges,
            int[] firstEdges,
            float maxSpeed)
        {
            _nodes = nodes;
            _edges = edges;
            _firstEdges = firstEdges;
            MaxSpeed = maxSpeed;
        }

        public static NavGraph Of(NavGraphAsset asset)
        {
            var nodes = asset.Nodes;
            var edges = (NavEdge[])asset.Edges.Clone();
            if (nodes.Length == 0)
                throw new InvalidOperationException($"Navigation graph '{asset.name}' has no nodes: bake it for its level (Tools → Bots → Bake Navigation).");

            var maxSpeed = 0f;
            foreach (var edge in edges)
            {
                if (edge.From < 0 || edge.From >= nodes.Length || edge.To < 0 || edge.To >= nodes.Length || edge.Ticks <= 0)
                    throw new InvalidOperationException($"Navigation graph '{asset.name}' has a broken edge {edge.From} → {edge.To} in {edge.Ticks} ticks: bake it again.");

                maxSpeed = Mathf.Max(maxSpeed, Mathf.Abs(nodes[edge.To].x - nodes[edge.From].x) / edge.Ticks);
            }

            Array.Sort(edges, ByStart);
            var firstEdges = new int[nodes.Length + 1];
            foreach (var edge in edges)
                firstEdges[edge.From + 1]++;

            for (var node = 0; node < nodes.Length; node++)
                firstEdges[node + 1] += firstEdges[node];

            return new NavGraph(nodes, edges, firstEdges, maxSpeed);
        }

        public Vector2 NodeAt(int node) =>
            _nodes[node];

        public int FirstEdgeOf(int node) =>
            _firstEdges[node];

        public int EdgeEndOf(int node) =>
            _firstEdges[node + 1];

        public NavEdge EdgeAt(int index) =>
            _edges[index];

        public bool TryFindStandingNode(Vector2 position, out int node)
        {
            node = -1;
            var closest = float.MaxValue;
            for (var index = 0; index < _nodes.Length; index++)
            {
                var delta = _nodes[index] - position;
                if (Mathf.Abs(delta.y) > StandingHeight || Mathf.Abs(delta.x) > StandingReach || Mathf.Abs(delta.x) >= closest)
                    continue;

                closest = Mathf.Abs(delta.x);
                node = index;
            }

            return node >= 0;
        }

        public bool TryFindNodeBelow(Vector2 position, out int node)
        {
            node = -1;
            var closest = float.MaxValue;
            for (var index = 0; index < _nodes.Length; index++)
            {
                var delta = _nodes[index] - position;
                var distance = delta.sqrMagnitude;
                if (delta.y > StandingHeight || distance >= closest)
                    continue;

                closest = distance;
                node = index;
            }

            return node >= 0;
        }

        public int NearestNode(Vector2 position)
        {
            var nearest = 0;
            var closest = float.MaxValue;
            for (var index = 0; index < _nodes.Length; index++)
            {
                var distance = (_nodes[index] - position).sqrMagnitude;
                if (distance >= closest)
                    continue;

                closest = distance;
                nearest = index;
            }

            return nearest;
        }

        private static int ByStart(NavEdge first, NavEdge second) =>
            first.From != second.From ? first.From.CompareTo(second.From) : first.Ticks.CompareTo(second.Ticks);
    }
}
