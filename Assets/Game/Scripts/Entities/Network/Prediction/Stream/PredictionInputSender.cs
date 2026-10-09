using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class PredictionInputSender<TInput> where TInput : struct
    {
        public ISource<TInput> Predicted => _predicted;

        private readonly Signal<TInput> _predicted = new();
        private readonly IInputSampler<TInput> _sampler;
        private readonly INetRequestSender<TickedInput<TInput>> _input;

        private TInput _previousInput;

        public PredictionInputSender(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            INetworkTicks ticks,
            IInputSampler<TInput> sampler,
            INetRequestSender<TickedInput<TInput>> input)
        {
            _sampler = sampler;
            _input = input;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                ticks.Ticked(TickPhase.Default).Advise(controlledLifetime, Send));
        }

        private void Send(int tick)
        {
            var input = _sampler.Sample();

            _input.Send(new TickedInput<TInput>(tick, input, _previousInput));
            _previousInput = input;
            _predicted.Fire(input);
        }
    }
}
