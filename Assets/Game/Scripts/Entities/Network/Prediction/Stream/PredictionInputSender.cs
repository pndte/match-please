using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class PredictionInputSender<TInput> where TInput : struct
    {
        private readonly IInputSampler<TInput> _sampler;
        private readonly INetRequestSender<TickedInput<TInput>> _input;
        private readonly ITickHistory<TInput> _own;

        private TInput _previousInput;

        public PredictionInputSender(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IWorldStep worldStep,
            IInputSampler<TInput> sampler,
            INetRequestSender<TickedInput<TInput>> input,
            ITickHistory<TInput> own)
        {
            _sampler = sampler;
            _input = input;
            _own = own;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                worldStep.Stepped(TickPhase.Prepare).Advise(controlledLifetime, Send));
        }

        private void Send(int tick)
        {
            var input = _sampler.Sample();

            _input.Send(new TickedInput<TInput>(tick, input, _previousInput));
            _own.Record(tick, input);
            _previousInput = input;
        }
    }
}
