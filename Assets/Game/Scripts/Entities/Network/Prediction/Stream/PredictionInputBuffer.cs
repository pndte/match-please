using System;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class PredictionInputBuffer<TInput> where TInput : struct
    {
        public ISource<InputGap> Gaps => _gaps;

        private readonly Signal<InputGap> _gaps = new();
        private readonly TickHistory<TInput> _buffered;
        private readonly INetworkTicks _ticks;
        private readonly NetworkTicksConfig _config;
        private readonly IInputPolicy<TInput> _policy;
        private readonly IInputContinuation<TInput> _continuation;

        private TInput _lastInput;
        private TInput _lastKnown;
        private bool _heard;
        private int _firstTick;
        private TInput _newest;
        private int _newestTick = int.MinValue;

        public PredictionInputBuffer(
            Lifetime lifetime,
            INetworkTicks ticks,
            NetworkTicksConfig config,
            IInputPolicy<TInput> policy,
            IInputContinuation<TInput> continuation,
            INetRequestReceiver<TickedInput<TInput>> input)
        {
            _ticks = ticks;
            _config = config;
            _policy = policy;
            _continuation = continuation;
            _buffered = new TickHistory<TInput>(config.MaxBufferedInputTicks + 1);

            input.Received.Advise(lifetime, Buffer);
        }

        public void Clear()
        {
            _buffered.Clear();
            _heard = false;
            _newestTick = int.MinValue;
            _lastInput = default;
        }

        public bool TryTakeInputAt(int tick, out TInput input)
        {
            if (!_heard || tick < _firstTick) //TODO: до первого ввода управляющего объект не шагает: только что появившийся персонаж висит в точке спавна около пинга, пока не придёт его ввод
            {
                input = default;
                return false;
            }

            if (_buffered.TryGetAt(tick, out var buffered))
            {
                _lastKnown = buffered;
                _lastInput = _policy.CaughtUp(buffered, _lastInput, buffered, 0);
            }
            else
            {
                _gaps.Fire(InputGap.Substituted);
                _lastKnown = _newestTick < tick ? _continuation.Continued(_newest, tick - _newestTick) : _continuation.Continued(_lastKnown, 1);
                _lastInput = _policy.CaughtUp(_lastKnown, _lastInput, _newest, tick - _newestTick);
            }

            input = _lastInput;
            return true;
        }

        private void Buffer(TickedInput<TInput> request)
        {
            ReportOutsideWindow(request.Tick);
            RememberNewest(request);
            if (!TryStore(request.Tick, request.Input))
                return;

            _firstTick = _heard ? Math.Min(_firstTick, request.Tick) : request.Tick;
            _heard = true;
            if (request.Tick - 1 >= _firstTick && TryStore(request.Tick - 1, request.PreviousInput))
                _gaps.Fire(InputGap.Recovered);
        }

        private void ReportOutsideWindow(int tick)
        {
            if (tick <= _ticks.Current)
                _gaps.Fire(InputGap.Late(_ticks.Current - tick + 1));
            else if (tick > _ticks.Current + _config.MaxBufferedInputTicks)
                _gaps.Fire(InputGap.Early);
        }

        private void RememberNewest(TickedInput<TInput> request)
        {
            if (request.Tick <= _newestTick || !_policy.IsValid(request.Input))
                return;

            _newest = request.Input;
            _newestTick = request.Tick;
        }

        private bool TryStore(int tick, TInput input)
        {
            if (tick <= _ticks.Current
                || tick > _ticks.Current + _config.MaxBufferedInputTicks
                || !_policy.IsValid(input)
                || _buffered.TryGetAt(tick, out _))
                return false;

            _buffered.Record(tick, input);
            return true;
        }
    }
}
