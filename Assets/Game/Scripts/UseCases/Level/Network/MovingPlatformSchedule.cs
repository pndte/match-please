using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Level.Network
{
    public sealed class MovingPlatformSchedule
    {
        private readonly MovingPlatform _platform;
        private readonly ISimulationStep _step;

        public MovingPlatformSchedule(
            Lifetime lifetime,
            MovingPlatform platform,
            ISimulationStep step,
            IWorldStep world)
        {
            _platform = platform;
            _step = step;

            world.Stepped(TickPhase.Early).Advise(lifetime, Pose);
        }

        private void Pose(int tick) =>
            _platform.PoseAt(tick * _step.Duration);
    }
}
