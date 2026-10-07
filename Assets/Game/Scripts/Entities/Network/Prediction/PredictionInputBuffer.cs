using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Requests;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction
{
    public sealed class PredictionInputBuffer<TInput> where TInput : struct
    {
        public ISource<TInput> Simulated => _simulated;

        private readonly Signal<TInput> _simulated = new();
        private readonly Dictionary<int, TInput> _buffered = new();
        private readonly INetworkTicks _ticks;
        private readonly NetworkTicksConfig _config;
        private readonly IInputPolicy<TInput> _policy;
        private readonly IControlledBy _controlledBy;

        private TInput _lastInput;

        public PredictionInputBuffer(
            Lifetime lifetime,
            INetworkTicks ticks,
            NetworkTicksConfig config,
            IInputPolicy<TInput> policy,
            IControlledBy controlledBy,
            IPredictionInputRequest<TInput> inputRequest)
        {
            _ticks = ticks;
            _config = config;
            _policy = policy;
            _controlledBy = controlledBy;

            controlledBy.Users.View(lifetime, (userLifetime, _) =>
            {
                _buffered.Clear();
                userLifetime.OnTermination(_buffered.Clear);
            });
            inputRequest.Requested.Advise(lifetime, Buffer);
            ticks.Ticked(TickPhase.Default).Advise(lifetime, Simulate);
        }

        private void Buffer(TickedInput<TInput> request)
        {
            Store(request.Tick, request.Input);
            Store(request.Tick - 1, request.PreviousInput);
        }

        private void Store(int tick, TInput input)
        {
            if (_controlledBy.Users.Count == 0 || tick <= _ticks.Current || tick > _ticks.Current + _config.MaxBufferedInputTicks || !_policy.IsValid(input))
                return;

            _buffered.TryAdd(tick, input);
        }

        private void Simulate(int tick)
        {
            if (_controlledBy.Users.Count == 0)
                _lastInput = default;
            else
                _lastInput = _buffered.Remove(tick, out var input) ? input : _policy.Substitute(_lastInput);
            _simulated.Fire(_lastInput);
        }
    }
}
