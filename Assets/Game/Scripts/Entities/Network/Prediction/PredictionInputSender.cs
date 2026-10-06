using Bw.Entities.Network.Prediction.Requests;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction
{
    public sealed class PredictionInputSender<TInput> where TInput : struct
    {
        public ISource<TInput> Predicted => _predicted;

        private readonly Signal<TInput> _predicted = new();
        private readonly IInputSampler<TInput> _sampler;
        private readonly IPredictionInputRequest<TInput> _inputRequest;

        private TInput _previousInput;

        public PredictionInputSender(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            INetworkTicks ticks,
            IInputSampler<TInput> sampler,
            IPredictionInputRequest<TInput> inputRequest)
        {
            _sampler = sampler;
            _inputRequest = inputRequest;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                ticks.Ticked(TickPhase.Default).Advise(controlledLifetime, Send));
        }

        private void Send(int tick)
        {
            var input = _sampler.Sample();

            _inputRequest.Requested.Fire(new TickedInput<TInput>(tick, input, _previousInput));
            _previousInput = input;
            _predicted.Fire(input);
        }
    }
}
