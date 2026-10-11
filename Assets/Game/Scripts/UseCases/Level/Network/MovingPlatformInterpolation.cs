using Bw.Entities.Network;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;

namespace Bw.UseCases.Level.Network
{
    public sealed class MovingPlatformInterpolation
    {
        private readonly MovingPlatform _platform;
        private readonly INetworkTicks _ticks;
        private readonly IInterpolationTicks _interpolation;

        public MovingPlatformInterpolation(
            Lifetime lifetime,
            MovingPlatform platform,
            INetworkTicks ticks,
            IInterpolationTicks interpolation,
            INetworkHolder networkHolder)
        {
            _platform = platform;
            _ticks = ticks;
            _interpolation = interpolation;

            networkHolder.NetworkManager.AdviseNotNull(lifetime, _ =>
                Observable.EveryUpdate(UnityFrameProvider.Update, lifetime).Subscribe(Show));
        }

        private void Show(Unit _) =>
            _platform.ShowAt(_ticks.PredictedViewTick(_interpolation) * _ticks.Duration);
    }
}
