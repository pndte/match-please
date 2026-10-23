using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Movement.Network.Requests;

namespace Bw.UseCases.Movement.Network.Root
{
    public sealed class RootedMovementSampler : IInputSampler<MovementControls>
    {
        private readonly IInputSampler<MovementControls> _sampler;
        private readonly IReadonlyMovementRoot _root;
        private readonly INetworkTicks _ticks;

        public RootedMovementSampler(IInputSampler<MovementControls> sampler, IReadonlyMovementRoot root, INetworkTicks ticks)
        {
            _sampler = sampler;
            _root = root;
            _ticks = ticks;
        }

        public MovementControls Sample()
        {
            var controls = _sampler.Sample();
            return _root.RootedAt(_ticks.Current) ? default : controls;
        }
    }
}
