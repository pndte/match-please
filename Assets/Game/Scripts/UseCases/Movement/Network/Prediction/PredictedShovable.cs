using System;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PredictedShovable<TState> : IShovable, IPredictedBody, IPredictedShove where TState : struct, IBodyState<TState>
    {
        private const int Capacity = 64;

        private readonly int[] _shoveTicks = Unrecorded();
        private readonly float[] _shoves = new float[Capacity];
        private readonly BodyState[] _path = new BodyState[Capacity];
        private readonly IMovementBody _body;
        private readonly IBodyMotor _motor;
        private readonly IReplicatedStates<TState> _states;
        private readonly INetworkTicks _ticks;
        private readonly IInterpolationTicks _interpolation;
        private readonly BodyState _spawned;

        private int _posedTick;
        private bool _reposed;
        private bool _traced;
        private int _basisTick;
        private BodyState _basis;
        private int _pathStart;
        private int _pathEnd;

        public PredictedShovable(
            Lifetime lifetime,
            IMovementBody body,
            IBodyMotor motor,
            IReplicatedStates<TState> states,
            INetworkTicks ticks,
            IInterpolationTicks interpolation)
        {
            _body = body;
            _motor = motor;
            _states = states;
            _ticks = ticks;
            _interpolation = interpolation;
            _spawned = new BodyState(body.Position, Vector2.zero, 0f, true);
            _posedTick = ticks.Current;

            ticks.Ticked(TickPhase.Early).Advise(lifetime, PoseFor);
        }

        public void PoseFor(int tick) //TODO: копия шагает от последнего известного состояния сервера со своими толчками и отдельно от других копий: чужие толчки и удары после него она узнаёт только снимками — тогда толкающему приходит поправка
        {
            _posedTick = tick;
            _reposed = true;

            var before = tick - 1;
            if (Shoving(before))
                _body.Place(StateAfter(before).Position);
        }

        public bool TryGetShown(out Vector2 position)
        {
            var shown = _ticks.PredictedViewTick(_interpolation);
            var from = (int)Math.Floor(shown);
            if (!Shoving(from))
            {
                position = default;
                return false;
            }

            var before = StateAfter(from).Position;
            position = Vector2.Lerp(before, StateAfter(from + 1).Position, (float)(shown - from));
            return true;
        }

        public float PossibleShove(Vector2 direction, float distance, IMovementCollider shover) =>
            _motor.PossibleShove(_body.Position, direction, distance, shover);

        public void Shove(Vector2 direction, float distance, IMovementCollider shover)
        {
            if (distance <= 0f)
                throw new ArgumentOutOfRangeException(nameof(distance), distance, "A shove moves a body forward, so its distance must be positive.");

            var from = _body.Position;
            _motor.MakeWay(from, direction, distance, shover);
            _body.Place(from + direction * distance);
            Record(direction.x * distance);
        }

        private bool Shoving(int tick) =>
            TryGetFirstShove(tick - Capacity, out _);

        private BodyState StateAfter(int tick)
        {
            Rebase(tick);
            if (tick <= _pathStart)
                return _path[Slot(_pathStart)];

            for (var next = _pathEnd + 1; next <= tick; next++)
                _path[Slot(next)] = _motor.Step(_path[Slot(next - 1)], new Vector2(ShoveAt(next), 0f));

            _pathEnd = Math.Max(_pathEnd, tick);
            return _path[Slot(tick)];
        }

        private void Rebase(int tick)
        {
            var replicated = _states.TryGetAt(tick, out var state);
            var basisTick = replicated ? state.Tick : int.MinValue;
            var basis = replicated ? state.State.Body : _spawned;
            if (_traced && basisTick == _basisTick && basis.Equals(_basis) && tick - _pathStart < Capacity)
                return;

            _traced = true;
            _basisTick = basisTick;
            _basis = basis;
            var earliest = tick - Capacity + 1;
            _pathStart = Math.Max(Resting(basis) && TryGetFirstShove(Math.Max(basisTick, earliest), out var first) ? first - 1 : basisTick, earliest);
            _pathEnd = _pathStart;
            _path[Slot(_pathStart)] = basis;
        }

        private void Record(float shoved) //TODO: толчок тика, который пересчёт после поправки сервера больше не повторил, остаётся записанным, пока снимки не дойдут до этого тика: до тех пор копия стоит дальше, чем ящик на сервере
        {
            var slot = Slot(_posedTick);
            if (_reposed || _shoveTicks[slot] != _posedTick)
            {
                _shoveTicks[slot] = _posedTick;
                _shoves[slot] = 0f;
            }

            _shoves[slot] += shoved;
            _reposed = false;

            if (_posedTick <= _pathStart)
                _traced = false;
            else
                _pathEnd = Math.Min(_pathEnd, _posedTick - 1);
        }

        private bool TryGetFirstShove(int after, out int first)
        {
            first = int.MaxValue;
            foreach (var shoveTick in _shoveTicks)
                if (shoveTick > after)
                    first = Math.Min(first, shoveTick);

            return first != int.MaxValue;
        }

        private float ShoveAt(int tick)
        {
            var slot = Slot(tick);
            return _shoveTicks[slot] == tick ? _shoves[slot] : 0f;
        }

        private static bool Resting(BodyState body) =>
            body.Grounded && body.Velocity == Vector2.zero && body.HorizontalPush == 0f;

        private static int Slot(int tick) =>
            (tick % Capacity + Capacity) % Capacity;

        private static int[] Unrecorded()
        {
            var ticks = new int[Capacity];
            Array.Fill(ticks, int.MinValue);
            return ticks;
        }
    }
}
