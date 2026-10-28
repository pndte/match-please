using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Level.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Level.Network
{
    public sealed class MovingPlatformStepper
    {
        private readonly MovingPlatform _platform;
        private readonly MovingPlatformPath _path;
        private readonly ISimulationStep _step;

        public MovingPlatformStepper(
            Lifetime lifetime,
            MovingPlatform platform,
            MovingPlatformPath path,
            ISimulationStep step,
            IWorldStep world)
        {
            _platform = platform;
            _path = path;
            _step = step;

            world.Stepped(TickPhase.BeforePhysics).Advise(lifetime, DriveTo);
        }

        private void DriveTo(int tick) =>
            _platform.DriveTo(_path.AlongAt(tick * (double)_step.Duration));
    }
}
