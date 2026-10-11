using Bw.Entities.Network.Ticks;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Level.Network
{
    public sealed class MovingPlatformSchedule : IScheduled
    {
        private readonly MovingPlatform _platform;
        private readonly INetworkTicks _ticks;

        public MovingPlatformSchedule(Lifetime lifetime, MovingPlatform platform, INetworkTicks ticks)
        {
            _platform = platform;
            _ticks = ticks;

            ticks.Ticked(TickPhase.Early).Advise(lifetime, Pose);
        }

        public void PoseAt(double tick) =>
            _platform.PoseAt(tick * _ticks.Duration);

        private void Pose(int tick) =>
            PoseAt(tick);
    }
}
