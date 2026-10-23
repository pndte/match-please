using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class PredictionInputSender<TInput> : IFedInputs<TInput> where TInput : struct
    {
        public ISource<TickedInput<TInput>> Fed => _fed;

        private readonly Signal<TickedInput<TInput>> _fed = new();
        private readonly IInputSampler<TInput> _sampler;
        private readonly INetRequestSender<TickedInput<TInput>> _input;
        private readonly ISteppable<TInput> _simulation;

        private TInput _previousInput;

        public PredictionInputSender(
            Lifetime lifetime,
            IControlledBy controlledBy,
            INetworkTicks ticks,
            IInputSampler<TInput> sampler,
            INetRequestSender<TickedInput<TInput>> input,
            ISteppable<TInput> simulation)
        {
            _sampler = sampler;
            _input = input;
            _simulation = simulation;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                ticks.Ticked(TickPhase.Default).Advise(controlledLifetime, Send));
        }

        private void Send(int tick)
        {
            var input = _sampler.Sample();
            var ticked = new TickedInput<TInput>(tick, input, _previousInput);

            _input.Send(ticked);
            _previousInput = input;
            _fed.Fire(ticked);
            _simulation.Step(input);
        }
    }
}
