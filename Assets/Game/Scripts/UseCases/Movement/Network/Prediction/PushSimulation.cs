using System;
using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Replication;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PushSimulation<TEffect> where TEffect : struct //TODO: в прогоне цель не бежит и не прыгает заново, только держит или отпустила начатый прыжок, а платформы стоят там, где они в текущем тике, и стоящего на них каждый тик сдвигают на свой сдвиг этого тика — отлёт бегущей в стену или к краю цели и отлёт с едущей платформы стрелок покажет неточно, и снимки потом дотянут
    {
        private const int Capacity = 120;

        public int SettledTick { get; private set; } = int.MaxValue;

        private readonly List<PredictedEffect<TEffect>> _pushes = new();
        private readonly Vector2[] _offsets = new Vector2[Capacity + 1];

        private readonly IMovementMotor _motor;
        private readonly IReplicatedStates<MovementState> _replicated;
        private readonly IEffectRules<MovementState, TEffect> _rules;

        private int _start = int.MaxValue;
        private int _simulated = int.MaxValue;
        private int _firstPush;
        private int _lastPush;
        private MovementInput _input;
        private MovementState _free;
        private MovementState _pushed;

        public PushSimulation(IMovementMotor motor, IReplicatedStates<MovementState> replicated, IEffectRules<MovementState, TEffect> rules)
        {
            _motor = motor;
            _replicated = replicated;
            _rules = rules;
        }

        public void Simulate(IReadOnlyList<PredictedEffect<TEffect>> pushes, int until)
        {
            if (pushes.Count == 0)
                throw new ArgumentException("A push simulation runs at least one push.", nameof(pushes));

            _pushes.Clear();
            for (var index = 0; index < pushes.Count; index++)
                _pushes.Add(pushes[index]);

            _firstPush = int.MaxValue;
            _lastPush = int.MinValue;
            foreach (var push in _pushes)
            {
                _firstPush = Math.Min(_firstPush, push.Action.Tick);
                _lastPush = Math.Max(_lastPush, push.Action.Tick);
            }

            Restart();
            Continue(until);
        }

        public void Continue(int until)
        {
            if (_pushes.Count == 0)
                throw new InvalidOperationException("A push simulation continues only the pushes it was given to simulate.");

            if (_replicated.TryGetAt(_firstPush - 1, out var start) && start.Tick != _start)
                Restart();

            while (_simulated < until && _simulated - _start < Capacity)
                Advance();

            if (_simulated != int.MaxValue && _simulated - _start >= Capacity)
                SettledTick = Math.Min(SettledTick, _simulated);
        }

        public Vector2 OffsetAt(double tick)
        {
            var whole = Math.Floor(tick);
            var index = (long)whole - _start;
            if (index < 0)
                return Vector2.zero;

            var last = _simulated - _start;
            if (index >= last)
                return _offsets[last];

            return Vector2.Lerp(_offsets[index], _offsets[index + 1], (float)(tick - whole));
        }

        private void Restart()
        {
            if (!_replicated.TryGetAt(_firstPush - 1, out var start))
            {
                _start = int.MaxValue;
                _simulated = int.MaxValue;
                SettledTick = int.MinValue;
                return;
            }

            _start = start.Tick;
            _simulated = start.Tick;
            _input = new MovementInput(0f, false, IsHoldingJump(start), false);
            _free = start.State;
            _pushed = start.State;
            _offsets[0] = Vector2.zero;
            SettledTick = int.MaxValue;
        }

        private void Advance()
        {
            var tick = _simulated + 1;
            _free = _motor.Step(_free, _input);
            _pushed = _motor.Step(_pushed, _input);
            foreach (var push in _pushes)
                if (push.Action.Tick == tick)
                    _pushed = _rules.Apply(_pushed, push.Value);

            _simulated = tick;
            _offsets[tick - _start] = _pushed.Body.Position - _free.Body.Position;
            if (tick >= _lastPush && IsResting(_free) && IsResting(_pushed))
                SettledTick = Math.Min(SettledTick, tick);
        }

        private bool IsHoldingJump(TickedState<MovementState> start)
        {
            if (!start.State.Jump.Rising)
                return false;

            if (!_replicated.TryGetAt(start.Tick - 1, out var before) || before.Tick != start.Tick - 1 || !before.State.Jump.Rising)
                return true;

            var held = _motor.Step(before.State, new MovementInput(0f, false, true, false));
            var released = _motor.Step(before.State, new MovementInput(0f, false, false, false));
            var observed = start.State.Body.Velocity.y;
            return Mathf.Abs(held.Body.Velocity.y - observed) <= Mathf.Abs(released.Body.Velocity.y - observed);
        }

        private static bool IsResting(MovementState state) =>
            state.Body.Grounded && state.Body.Velocity == Vector2.zero && state.Body.HorizontalPush == 0f;
    }
}
