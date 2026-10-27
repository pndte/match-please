using System.Collections.Generic;
using Bw.UseCases.Movement.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Bots.Navigation
{
    public sealed class BotNavigator
    {
        private const int RepathTicks = 20;
        private const int WanderAttempts = 8;

        public NavStatus Status => _follower.Status;

        private readonly List<NavEdge> _path = new();

        private readonly NavGraph _graph;
        private readonly NavPathfinder _pathfinder;
        private readonly PathFollower _follower;
        private readonly IReadonlyMovement _movement;

        private bool _heading;
        private int _destination;
        private int _ticks;

        public BotNavigator(
            NavGraph graph,
            NavPathfinder pathfinder,
            PathFollower follower,
            IReadonlyMovement movement)
        {
            _graph = graph;
            _pathfinder = pathfinder;
            _follower = follower;
            _movement = movement;
        }

        public bool TryGoTo(Vector2 destination)
        {
            var node = _graph.NearestNode(destination);
            var repath = !_heading || node != _destination || _follower.Status != NavStatus.Moving || ++_ticks % RepathTicks == 0;
            return !repath || _follower.Leaping || TryHead(node);
        }

        public bool TryWander()
        {
            for (var attempt = 0; attempt < WanderAttempts; attempt++)
                if (TryHead(Random.Range(0, _graph.NodeCount)))
                    return true;

            return false;
        }

        public void Steer() =>
            _follower.Steer();

        public void Stop()
        {
            _follower.Stop();
            _heading = false;
        }

        private bool TryFindStart(Vector2 position, out int node) =>
            _graph.TryFindStandingNode(position, out node) || _graph.TryFindNodeBelow(position, out node);

        private bool TryHead(int destination)
        {
            if (!TryFindStart(_movement.State.Value.Body.Position, out var from) || !_pathfinder.TryFind(from, destination, _path))
                return false;

            _follower.Follow(_path);
            _heading = true;
            _destination = destination;
            _ticks = 0;
            return true;
        }
    }
}
