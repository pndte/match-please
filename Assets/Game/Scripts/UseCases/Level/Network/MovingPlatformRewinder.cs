using System;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Ticks;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Level.Network
{
    public sealed class MovingPlatformRewinder : IRewindable
    {
        private readonly MovingPlatform _platform;
        private readonly INetworkTicks _ticks;
        private readonly Action _restore;

        public MovingPlatformRewinder(
            Lifetime lifetime,
            MovingPlatform platform,
            INetworkTicks ticks,
            IRewindables rewindables)
        {
            _platform = platform;
            _ticks = ticks;
            _restore = Restore;

            rewindables.Track(lifetime, this);
        }

        public void Rewind(Lifetime lifetime, ShooterView view)
        {
            _platform.ShowAt(view.Predicted * _ticks.Duration);
            lifetime.OnTermination(_restore);
        }

        private void Restore() =>
            _platform.ShowAt(_ticks.Current * _ticks.Duration);
    }
}
