using System;
using System.Collections.Generic;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Bots.Navigation.Extensions;
using Bw.UseCases.Level;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Physics;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Bots.Navigation
{
    public sealed class NavGraphBuilder
    {
        private const float StartClearance = 0.02f;
        private const int SettleTicks = 6;
        private const float SettleReach = 0.2f;
        private const float LandingReach = 0.75f;
        private const float LandingHeight = 0.3f;

        private readonly NavGraphConfig _config;
        private readonly PhysicsConfig _physics;
        private readonly ISimulationStep _step;

        public NavGraphBuilder(NavGraphConfig config, PhysicsConfig physics, ISimulationStep step)
        {
            _config = config;
            _physics = physics;
            _step = step;
        }

        public NavGraphAsset Build(IReadOnlyList<BodyBox> level)
        {
            var lifetime = new LifetimeDefinition();
            try
            {
                var space = PhysicsSpace.Create(lifetime.Lifetime, _physics, _step, new SweptBodyCollection());
                LevelGeometry.Create(lifetime.Lifetime, space, level);
                var body = space.CreateBody(lifetime.Lifetime, _config.Character, PhysicsBody.BodyType.Kinematic);
                var motor = MotorIn(body);
                var nodes = NodesOn(level, space.World, body, motor);
                var edges = Pruned(nodes.Count, EdgesBetween(nodes, motor));
                var kept = LargestRoundTrip(nodes.Count, edges);
                var graph = ScriptableObject.CreateInstance<NavGraphAsset>();
                graph.Fingerprint = NavFingerprint.Of(level, _config, _step.Duration);
                graph.Nodes = KeptNodes(nodes, kept);
                graph.Edges = KeptEdges(edges, kept);
                return graph;
            }
            finally
            {
                lifetime.Terminate();
            }
        }

        private IMovementMotor MotorIn(PhysicsBody body)
        {
            var character = _config.Character;
            var movement = _config.Movement;
            var collider = new ShapeCollider(body, character, movement.GroundLayer, new PlatformRider(character, new MovingPlatformCollection()), new ShovableCollection());
            return new PlatformerMotor(new BodyMotor(collider, _step, movement), _step, movement);
        }

        private List<Vector2> NodesOn(
            IReadOnlyList<BodyBox> level,
            PhysicsWorld world,
            PhysicsBody body,
            IMovementMotor motor)
        {
            var size = _config.Character.WorldSize();
            var offset = _config.Character.WorldOffset();
            var solids = new PhysicsQuery.QueryFilter(PhysicsMask.All, _config.Movement.GroundLayer.AsPhysicsMask());
            var nodes = new List<Vector2>();
            var taken = new HashSet<(int, int)>();
            foreach (var box in level)
            {
                var rect = box.WorldRect();
                if (rect.width < size.x * 0.5f)
                    continue;

                var from = rect.xMin + size.x * 0.5f;
                var to = rect.xMax - size.x * 0.5f;
                var count = Mathf.Max(1, Mathf.CeilToInt((to - from) / _config.Spacing));
                for (var index = 0; index <= count; index++)
                {
                    var at = new Vector2(Mathf.Lerp(from, to, index / (float)count), rect.yMax + size.y * 0.5f - offset.y + StartClearance);
                    var standing = PolygonGeometry.CreateBox(size - Vector2.one * (StartClearance * 2f), 0f, new PhysicsTransform(at + offset), false);
                    if (!world.IsFreeOfSolids(standing, solids, body))
                        continue;

                    var state = Standing(at);
                    for (var tick = 0; tick < SettleTicks; tick++)
                        state = motor.Step(state, default);

                    if (!state.Body.Grounded || (state.Body.Position - at).magnitude > SettleReach)
                        continue;

                    var place = state.Body.Position;
                    if (taken.Add((Mathf.RoundToInt(place.x / (_config.Spacing * 0.5f)), Mathf.RoundToInt(place.y * 4f))))
                        nodes.Add(place);
                }
            }

            return nodes;
        }

        private List<NavEdge> EdgesBetween(List<Vector2> nodes, IMovementMotor motor)
        {
            var cheapest = new Dictionary<(int, int), NavEdge>();
            for (var node = 0; node < nodes.Count; node++)
                foreach (var direction in new[] { -1f, 1f })
                {
                    Walk(nodes, motor, node, direction, cheapest);
                    foreach (var share in _config.JumpShares)
                    {
                        if (share == 0f && direction < 0f)
                            continue;

                        foreach (var hold in _config.JumpHolds)
                        foreach (var delay in _config.RunDelays)
                            if (share > 0f || delay == 0)
                                Leap(nodes, motor, node, new NavManeuver { Horizontal = share * direction, JumpTicks = hold, RunDelay = delay }, cheapest);

                        Leap(nodes, motor, node, new NavManeuver { Horizontal = share * direction, Dropping = true }, cheapest);
                    }
                }

            return new List<NavEdge>(cheapest.Values);
        }

        private void Walk(
            List<Vector2> nodes,
            IMovementMotor motor,
            int node,
            float direction,
            Dictionary<(int, int), NavEdge> cheapest)
        {
            var maneuver = new NavManeuver { Horizontal = direction };
            var state = Standing(nodes[node]);
            var limit = Mathf.CeilToInt(_config.Spacing * 2f / (_config.Movement.Speed * _step.Duration));
            for (var tick = 1; tick <= limit; tick++)
            {
                state = motor.Step(state, maneuver.InputAt(tick - 1));
                if (!state.Body.Grounded)
                {
                    if (TryLand(nodes, motor, state, maneuver, tick, out var landed, out var ticks))
                        Keep(cheapest, node, landed, ticks, maneuver);

                    return;
                }

                if (Mathf.Abs(state.Body.Position.x - nodes[node].x) >= _config.Spacing * 0.9f)
                {
                    if (TryFindNear(nodes, state.Body.Position, out var reached))
                        Keep(cheapest, node, reached, tick, maneuver);

                    return;
                }
            }
        }

        private void Leap(
            List<Vector2> nodes,
            IMovementMotor motor,
            int node,
            NavManeuver maneuver,
            Dictionary<(int, int), NavEdge> cheapest)
        {
            if (TryLand(nodes, motor, Standing(nodes[node]), maneuver, 0, out var landed, out var ticks))
                Keep(cheapest, node, landed, ticks, maneuver);
        }

        private bool TryLand(
            List<Vector2> nodes,
            IMovementMotor motor,
            MovementState state,
            NavManeuver maneuver,
            int tick,
            out int landed,
            out int ticks)
        {
            var airborne = !state.Body.Grounded;
            for (ticks = tick + 1; ticks <= tick + _config.MaxFlightTicks; ticks++)
            {
                state = motor.Step(state, maneuver.InputAt(ticks - 1));
                if (!state.Body.Grounded)
                    airborne = true;
                else if (airborne)
                    return TryFindNear(nodes, state.Body.Position, out landed);
            }

            landed = -1;
            return false;
        }

        private static bool TryFindNear(List<Vector2> nodes, Vector2 position, out int found)
        {
            found = -1;
            var closest = float.MaxValue;
            for (var index = 0; index < nodes.Count; index++)
            {
                var delta = nodes[index] - position;
                if (Mathf.Abs(delta.y) > LandingHeight || Mathf.Abs(delta.x) > LandingReach || delta.sqrMagnitude >= closest)
                    continue;

                closest = delta.sqrMagnitude;
                found = index;
            }

            return found >= 0;
        }

        private static void Keep(
            Dictionary<(int, int), NavEdge> cheapest,
            int from,
            int to,
            int ticks,
            NavManeuver maneuver)
        {
            if (to == from || cheapest.TryGetValue((from, to), out var known) && known.Ticks <= ticks)
                return;

            cheapest[(from, to)] = new NavEdge { From = from, To = to, Ticks = ticks, Maneuver = maneuver };
        }

        private static List<NavEdge> Pruned(int nodeCount, List<NavEdge> edges)
        {
            edges.Sort((first, second) => second.Ticks.CompareTo(first.Ticks));
            var outgoing = new List<int>[nodeCount];
            for (var node = 0; node < nodeCount; node++)
                outgoing[node] = new List<int>();

            for (var index = 0; index < edges.Count; index++)
                outgoing[edges[index].From].Add(index);

            var removed = new bool[edges.Count];
            var costs = new Dictionary<int, int>();
            for (var index = 0; index < edges.Count; index++)
                removed[index] = IsBypassed(edges, outgoing, removed, index, costs);

            var kept = new List<NavEdge>();
            for (var index = 0; index < edges.Count; index++)
                if (!removed[index])
                    kept.Add(edges[index]);

            return kept;
        }

        private static bool IsBypassed(
            List<NavEdge> edges,
            List<int>[] outgoing,
            bool[] removed,
            int skipped,
            Dictionary<int, int> costs)
        {
            var edge = edges[skipped];
            costs.Clear();
            costs[edge.From] = 0;
            var frontier = new List<int> { edge.From };
            while (frontier.Count > 0)
            {
                var best = 0;
                for (var index = 1; index < frontier.Count; index++)
                    if (costs[frontier[index]] < costs[frontier[best]])
                        best = index;

                var node = frontier[best];
                frontier.RemoveAt(best);
                if (node == edge.To)
                    return true;

                foreach (var next in outgoing[node])
                {
                    if (next == skipped || removed[next])
                        continue;

                    var cost = costs[node] + edges[next].Ticks;
                    if (cost > edge.Ticks || costs.TryGetValue(edges[next].To, out var known) && known <= cost)
                        continue;

                    if (!costs.ContainsKey(edges[next].To))
                        frontier.Add(edges[next].To);

                    costs[edges[next].To] = cost;
                }
            }

            return false;
        }

        private static int[] LargestRoundTrip(int nodeCount, List<NavEdge> edges)
        {
            var outgoing = Links(nodeCount, edges, false);
            var incoming = Links(nodeCount, edges, true);
            var order = new List<int>(nodeCount);
            var visited = new bool[nodeCount];
            for (var node = 0; node < nodeCount; node++)
                if (!visited[node])
                    Finish(node, outgoing, visited, order);

            var components = new int[nodeCount];
            Array.Fill(components, -1);
            var sizes = new List<int>();
            for (var index = order.Count - 1; index >= 0; index--)
                if (components[order[index]] < 0)
                    sizes.Add(Mark(order[index], incoming, components, sizes.Count));

            var largest = sizes.IndexOf(Mathf.Max(sizes.ToArray()));
            var kept = new int[nodeCount];
            var next = 0;
            for (var node = 0; node < nodeCount; node++)
                kept[node] = components[node] == largest ? next++ : -1;

            return kept;
        }

        private static List<int>[] Links(int nodeCount, List<NavEdge> edges, bool reversed)
        {
            var links = new List<int>[nodeCount];
            for (var node = 0; node < nodeCount; node++)
                links[node] = new List<int>();

            foreach (var edge in edges)
                links[reversed ? edge.To : edge.From].Add(reversed ? edge.From : edge.To);

            return links;
        }

        private static void Finish(
            int start,
            List<int>[] outgoing,
            bool[] visited,
            List<int> order)
        {
            var stack = new Stack<(int node, int next)>();
            visited[start] = true;
            stack.Push((start, 0));
            while (stack.Count > 0)
            {
                var (node, next) = stack.Pop();
                if (next < outgoing[node].Count)
                {
                    stack.Push((node, next + 1));
                    var child = outgoing[node][next];
                    if (!visited[child])
                    {
                        visited[child] = true;
                        stack.Push((child, 0));
                    }

                    continue;
                }

                order.Add(node);
            }
        }

        private static int Mark(
            int start,
            List<int>[] incoming,
            int[] components,
            int component)
        {
            var size = 0;
            var stack = new Stack<int>();
            components[start] = component;
            stack.Push(start);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                size++;
                foreach (var previous in incoming[node])
                    if (components[previous] < 0)
                    {
                        components[previous] = component;
                        stack.Push(previous);
                    }
            }

            return size;
        }

        private static Vector2[] KeptNodes(List<Vector2> nodes, int[] kept)
        {
            var result = new List<Vector2>();
            for (var node = 0; node < nodes.Count; node++)
                if (kept[node] >= 0)
                    result.Add(nodes[node]);

            return result.ToArray();
        }

        private static NavEdge[] KeptEdges(List<NavEdge> edges, int[] kept)
        {
            var result = new List<NavEdge>();
            foreach (var edge in edges)
                if (kept[edge.From] >= 0 && kept[edge.To] >= 0)
                    result.Add(new NavEdge { From = kept[edge.From], To = kept[edge.To], Ticks = edge.Ticks, Maneuver = edge.Maneuver });

            return result.ToArray();
        }

        private static MovementState Standing(Vector2 at) =>
            new(new BodyState(at, Vector2.zero, 0f, true), new JumpState(false, 0, 0));
    }
}
