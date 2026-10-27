using System.Collections.Generic;
using Bw.Entities.Simulation;
using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Bots.Navigation.Extensions;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Bots.Navigation
{
    public sealed class PathFollower //TODO: лифты, подъёмник и патерностеры граф не знает: бот их не вызывает и не ждёт, а если платформа окажется на пути, ребро сорвётся и путь строится заново
    {
        private const float Aligned = 0.05f;
        private const float Resting = 0.01f;
        private const float PressedReach = 0.1f;
        private const float Stalled = 0.001f;
        private const float Arrival = 0.1f;
        private const float LandingReach = 0.75f;
        private const float LandingHeight = 0.3f;
        private const int SlackTicks = 30;

        public NavStatus Status { get; private set; } = NavStatus.Idle;
        public bool Leaping => Status == NavStatus.Moving && _leaping;

        private readonly List<NavEdge> _path = new();
        private readonly NavGraph _graph;
        private readonly IReadonlyMovement _movement;
        private readonly IBotGamepad _gamepad;
        private readonly MovementConfig _config;
        private readonly ISimulationStep _step;

        private int _index;
        private int _edgeTicks;
        private bool _aligning;
        private bool _stalled;
        private float _alignedX;
        private bool _leaping;
        private bool _airborne;
        private int _leapTick;

        public PathFollower(
            NavGraph graph,
            IReadonlyMovement movement,
            IBotGamepad gamepad,
            MovementConfig config,
            ISimulationStep step)
        {
            _graph = graph;
            _movement = movement;
            _gamepad = gamepad;
            _config = config;
            _step = step;
        }

        public void Follow(IReadOnlyList<NavEdge> path)
        {
            _path.Clear();
            for (var index = 0; index < path.Count; index++)
                _path.Add(path[index]);

            _index = 0;
            BeginEdge();
            Status = _path.Count == 0 ? NavStatus.Arrived : NavStatus.Moving;
        }

        public void Stop()
        {
            _path.Clear();
            Status = NavStatus.Idle;
        }

        public void Steer()
        {
            if (Status != NavStatus.Moving)
                return;

            if (++_edgeTicks > _path[_index].Ticks * 2 + SlackTicks)
            {
                Status = NavStatus.Lost;
                return;
            }

            var body = _movement.State.Value.Body;
            if (_path[_index].Maneuver.IsRun())
                Run(body);
            else
                Leap(body);
        }

        private void Run(BodyState body)
        {
            var end = RunEnd();
            var direction = Mathf.Sign(_path[_index].Maneuver.Horizontal);
            while (_index < end && IsPast(body.Position.x, _graph.NodeAt(_path[_index].To).x, direction))
                Advance();

            var target = _graph.NodeAt(_path[end].To);
            if (body.Grounded && Mathf.Abs(target.x - body.Position.x) <= Arrival && Mathf.Abs(target.y - body.Position.y) <= LandingHeight)
            {
                _index = end;
                Advance();
                return;
            }

            _gamepad.Steer(new MovementInput(Toward(target.x, body.Position.x), false, false, false, Slowdown.None));
        }

        private void Leap(BodyState body)
        {
            var edge = _path[_index];
            if (!_leaping)
            {
                var start = _graph.NodeAt(edge.From);
                var distance = Mathf.Abs(start.x - body.Position.x);
                _stalled |= _aligning && distance <= PressedReach && Mathf.Abs(body.Position.x - _alignedX) < Stalled && Mathf.Abs(body.Velocity.x) > Resting;
                if (!body.Grounded || distance > Aligned && !_stalled || Mathf.Abs(body.Velocity.x) > Resting)
                {
                    _aligning = true;
                    _alignedX = body.Position.x;
                    _gamepad.Steer(new MovementInput(_stalled ? 0f : Toward(start.x, body.Position.x), false, false, false, Slowdown.None));
                    return;
                }

                _leaping = true;
            }

            if (!body.Grounded)
            {
                _airborne = true;
            }
            else if (_airborne)
            {
                if (Mathf.Abs(body.Velocity.x) > Resting)
                    _gamepad.Steer(new MovementInput(0f, false, false, false, Slowdown.None));
                else
                    Land(body, _graph.NodeAt(edge.To));

                return;
            }

            _gamepad.Steer(edge.Maneuver.InputAt(_leapTick++));
        }

        private void Land(BodyState body, Vector2 target)
        {
            if (Mathf.Abs(target.x - body.Position.x) <= LandingReach && Mathf.Abs(target.y - body.Position.y) <= LandingHeight)
                Advance();
            else
                Status = NavStatus.Lost;
        }

        private int RunEnd()
        {
            var end = _index;
            var direction = Mathf.Sign(_path[_index].Maneuver.Horizontal);
            while (end + 1 < _path.Count && _path[end + 1].Maneuver.IsRun() && Mathf.Sign(_path[end + 1].Maneuver.Horizontal) == direction)
                end++;

            return end;
        }

        private void Advance()
        {
            _index++;
            BeginEdge();
            if (_index >= _path.Count)
                Status = NavStatus.Arrived;
        }

        private void BeginEdge()
        {
            _edgeTicks = 0;
            _aligning = false;
            _stalled = false;
            _leaping = false;
            _airborne = false;
            _leapTick = 0;
        }

        private float Toward(float x, float from)
        {
            var distance = x - from;
            var speed = Mathf.Min(Mathf.Abs(distance) / _step.Duration, StoppingSpeed(Mathf.Abs(distance)));
            return Mathf.Clamp(Mathf.Sign(distance) * speed / _config.Speed, -1f, 1f);
        }

        private float StoppingSpeed(float distance)
        {
            if (_config.BrakingTime <= 0f)
                return float.PositiveInfinity;

            var braking = _config.Speed / _config.BrakingTime;
            var half = _step.Duration / 2f;
            return braking * (Mathf.Sqrt(half * half + 2f * distance / braking) - half);
        }

        private static bool IsPast(float x, float node, float direction) =>
            direction > 0f ? x >= node - Arrival : x <= node + Arrival;
    }
}
