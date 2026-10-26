using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Serialization;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class WireRoundedSampler<TInput, TCodec> : IInputSampler<TInput>
        where TInput : struct
        where TCodec : struct, ICodec<TickedInput<TInput>>
    {
        private readonly IInputSampler<TInput> _sampler;

        public WireRoundedSampler(IInputSampler<TInput> sampler)
        {
            _sampler = sampler;
        }

        public TInput Sample() =>
            CodecRoundTrip.Received<TickedInput<TInput>, TCodec>(new TickedInput<TInput>(0, _sampler.Sample(), default)).Input;
    }
}
