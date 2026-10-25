using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class PredictionInputSender<TInput> where TInput : struct //TODO: при задержке ввода выстрел и удар уходят на её тики позже нажатия, и клиент, и сервер судят их на тике применения: по бегущей цели нужно упреждение; полная компенсация — перематывать цели к тику нажатия на обоих пирах
    {
        private readonly IInputSampler<TInput> _sampler;
        private readonly INetRequestSender<TickedInput<TInput>> _input;
        private readonly ITickHistory<TInput> _own;
        private readonly IInputDelay _delay;
        private readonly IInputContinuation<TInput> _continuation;

        private TInput _previousInput;
        private int _sentTick = int.MinValue;

        public PredictionInputSender( //TODO: чрезвычайное к-во параметров
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IWorldStep worldStep,
            IInputSampler<TInput> sampler,
            INetRequestSender<TickedInput<TInput>> input,
            ITickHistory<TInput> own,
            IInputDelay delay,
            IInputContinuation<TInput> continuation)
        {
            _sampler = sampler;
            _input = input;
            _own = own;
            _delay = delay;
            _continuation = continuation;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                worldStep.Stepped(TickPhase.Prepare).Advise(controlledLifetime, Send));
        }

        private void Send(int tick)
        {
            var target = tick + _delay.Ticks.Value;
            if (target <= _sentTick)
                return;

            var input = _sampler.Sample();
            if (target == _sentTick + 2)
            {
                Transmit(target - 1, input);
                input = _continuation.Continued(input, 1);
            }

            Transmit(target, input);
        }

        private void Transmit(int tick, TInput input)
        {
            _input.Send(new TickedInput<TInput>(tick, input, _previousInput));
            _own.Record(tick, input);
            _previousInput = input;
            _sentTick = tick;
        }
    }
}
