using System.Collections.Generic;
using UnityEngine;

namespace Bw.UseCases.Bots.Navigation
{
    public sealed class NavPathfinder
    {
        private readonly NavGraph _graph;
        private readonly float[] _costs;
        private readonly int[] _arrivals;
        private readonly int[] _stamps;
        private readonly bool[] _closed;
        private readonly List<(float priority, int node)> _open = new();

        private int _stamp;

        public NavPathfinder(NavGraph graph)
        {
            _graph = graph;
            _costs = new float[graph.NodeCount];
            _arrivals = new int[graph.NodeCount];
            _stamps = new int[graph.NodeCount];
            _closed = new bool[graph.NodeCount];
        }

        public bool TryFind(int from, int to, List<NavEdge> path)
        {
            path.Clear();
            _stamp++;
            _open.Clear();
            Reach(from, 0f, -1);
            Push(from, Estimate(from, to));
            while (_open.Count > 0)
            {
                var node = Pop();
                if (_closed[node])
                    continue;

                if (node == to)
                {
                    Unwind(to, path);
                    return true;
                }

                _closed[node] = true;
                for (var index = _graph.FirstEdgeOf(node); index < _graph.EdgeEndOf(node); index++)
                {
                    var edge = _graph.EdgeAt(index);
                    var cost = _costs[node] + edge.Ticks;
                    if (_stamps[edge.To] == _stamp && cost >= _costs[edge.To])
                        continue;

                    Reach(edge.To, cost, index);
                    Push(edge.To, cost + Estimate(edge.To, to));
                }
            }

            return false;
        }

        private void Reach(int node, float cost, int edge)
        {
            if (_stamps[node] != _stamp)
            {
                _stamps[node] = _stamp;
                _closed[node] = false;
            }

            _costs[node] = cost;
            _arrivals[node] = edge;
        }

        private float Estimate(int node, int to) =>
            _graph.MaxSpeed > 0f ? Mathf.Abs(_graph.NodeAt(to).x - _graph.NodeAt(node).x) / _graph.MaxSpeed : 0f;

        private void Unwind(int to, List<NavEdge> path)
        {
            for (var arrival = _arrivals[to]; arrival >= 0; arrival = _arrivals[_graph.EdgeAt(arrival).From])
                path.Add(_graph.EdgeAt(arrival));

            path.Reverse();
        }

        private void Push(int node, float priority)
        {
            _open.Add((priority, node));
            var child = _open.Count - 1;
            while (child > 0)
            {
                var parent = (child - 1) / 2;
                if (_open[parent].priority <= _open[child].priority)
                    break;

                (_open[parent], _open[child]) = (_open[child], _open[parent]);
                child = parent;
            }
        }

        private int Pop()
        {
            var top = _open[0].node;
            var last = _open.Count - 1;
            _open[0] = _open[last];
            _open.RemoveAt(last);
            var parent = 0;
            while (true)
            {
                var smallest = parent;
                var left = parent * 2 + 1;
                var right = left + 1;
                if (left < _open.Count && _open[left].priority < _open[smallest].priority)
                    smallest = left;

                if (right < _open.Count && _open[right].priority < _open[smallest].priority)
                    smallest = right;

                if (smallest == parent)
                    return top;

                (_open[parent], _open[smallest]) = (_open[smallest], _open[parent]);
                parent = smallest;
            }
        }
    }
}
