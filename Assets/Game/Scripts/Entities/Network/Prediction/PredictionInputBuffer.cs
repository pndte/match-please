using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Requests;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction
{
    public sealed class PredictionInputBuffer<TInput> where TInput : struct
    {
        private const int MaxBufferedTicks = 32;

        public ISource<TInput> Simulated => _simulated;

        private readonly Signal<TInput> _simulated = new();
        private readonly Dictionary<int, TInput> _buffered = new();
        private readonly INetworkTicks _ticks;
        private readonly IInputPolicy<TInput> _policy;

        private TInput _lastInput;

        public PredictionInputBuffer(
            Lifetime lifetime,
            INetworkTicks ticks,
            IInputPolicy<TInput> policy,
            IPredictionInputRequest<TInput> inputRequest)
        {
            _ticks = ticks;
            _policy = policy;

            inputRequest.Requested.Advise(lifetime, Buffer);
            ticks.Ticked.Advise(lifetime, Simulate);
        }

        private void Buffer(TickedInput<TInput> request)
        {
            Store(request.Tick, request.Input);
            Store(request.Tick - 1, request.PreviousInput);
        }

        private void Store(int tick, TInput input)
        {
            if (tick <= _ticks.Current || tick > _ticks.Current + MaxBufferedTicks || !_policy.IsValid(input))
                return;

            _buffered.TryAdd(tick, input); //TODO: при смене управляющего вводы прежнего на несколько тиков вперёд остаются в буфере и применятся к новому (станет важно с подбором оружия) — чистить буфер при смене управления
        }

        private void Simulate(int tick)
        {
            _lastInput = _buffered.Remove(tick, out var input) ? input : _policy.Substitute(_lastInput);
            _simulated.Fire(_lastInput);
        }
    }
}
