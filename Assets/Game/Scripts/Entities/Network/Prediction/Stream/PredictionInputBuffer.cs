using System;
using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class PredictionInputBuffer<TInput> where TInput : struct
    {
        public ISource<TInput> Simulated => _simulated;
        public ISource<InputGap> Gaps => _gaps;

        private readonly Signal<TInput> _simulated = new();
        private readonly Signal<InputGap> _gaps = new();
        private readonly Dictionary<int, TInput> _buffered = new();
        private readonly INetworkTicks _ticks;
        private readonly NetworkTicksConfig _config;
        private readonly IInputPolicy<TInput> _policy;
        private readonly IControlledBy _controlledBy;

        private TInput _lastInput;
        private TInput _lastKnown;
        private bool _heard;
        private int _firstTick;
        private TInput _newest;
        private int _newestTick;

        public PredictionInputBuffer(
            Lifetime lifetime,
            INetworkTicks ticks,
            NetworkTicksConfig config,
            IInputPolicy<TInput> policy,
            IControlledBy controlledBy,
            INetRequestReceiver<TickedInput<TInput>> input)
        {
            _ticks = ticks;
            _config = config;
            _policy = policy;
            _controlledBy = controlledBy;

            controlledBy.Users.View(lifetime, (userLifetime, _) =>
            {
                _buffered.Clear();
                _heard = false;
                _newestTick = int.MinValue;
                _lastInput = default;
                userLifetime.OnTermination(_buffered.Clear);
            });
            input.Received.Advise(lifetime, Buffer);
            ticks.Ticked(TickPhase.Default).Advise(lifetime, Simulate);
        }

        private void Buffer(TickedInput<TInput> request)
        {
            if (_controlledBy.Users.Count > 0)
            {
                ReportOutsideWindow(request.Tick);
                RememberNewest(request);
            }

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

        private bool TryStore(int tick, TInput input) =>
            _controlledBy.Users.Count > 0
            && tick > _ticks.Current
            && tick <= _ticks.Current + _config.MaxBufferedInputTicks
            && _policy.IsValid(input)
            && _buffered.TryAdd(tick, input);

        private void Simulate(int tick)
        {
            if (_controlledBy.Users.Count == 0)
            {
                _lastInput = default;
            }
            else if (!_heard || tick < _firstTick) //TODO: до первого ввода управляющего объект не шагает: только что появившийся персонаж висит в точке спавна около пинга, пока не придёт его ввод
            {
                return;
            }
            else if (_buffered.Remove(tick, out var input))
            {
                _lastKnown = input;
                _lastInput = _policy.CaughtUp(input, _lastInput, input, 0);
            }
            else
            {
                _gaps.Fire(InputGap.Substituted);
                _lastKnown = _policy.Substitute(_newestTick < tick ? _newest : _lastKnown);
                _lastInput = _policy.CaughtUp(_lastKnown, _lastInput, _newest, tick - _newestTick);
            }

            _simulated.Fire(_lastInput);
        }
    }
}
