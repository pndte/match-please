using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Ticks;

namespace Bw.UseCases.Movement.Network.Root
{
    public sealed class RootedMovementSampler : IInputSampler<MovementInput>
    {
        private readonly IInputSampler<MovementInput> _sampler;
        private readonly IReadonlyMovementRoot _root;
        private readonly INetworkTicks _ticks;

        public RootedMovementSampler(IInputSampler<MovementInput> sampler, IReadonlyMovementRoot root, INetworkTicks ticks)
        {
            _sampler = sampler;
            _root = root;
            _ticks = ticks;
        }

        public MovementInput Sample()
        {
            var input = _sampler.Sample();
            return _root.RootedAt(_ticks.Current) ? default : input;
        }
    }
}
