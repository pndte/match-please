using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Level.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Level.Network
{
    public sealed class PredictedPlatformStepper
    {
        private readonly MovingPlatform _platform;
        private readonly MovingPlatformPath _path;
        private readonly ISimulationStep _step;

        public PredictedPlatformStepper(
            Lifetime lifetime,
            MovingPlatform platform,
            MovingPlatformPath path,
            ISimulationStep step,
            IPredictedBy predictedBy,
            IWorldStep world)
        {
            _platform = platform;
            _path = path;
            _step = step;

            predictedBy.Me.WhenTrue(lifetime, predictedLifetime =>
                world.Stepped(TickPhase.BeforePhysics).Advise(predictedLifetime, DriveTo));
        }

        private void DriveTo(int tick) =>
            _platform.DriveTo(_path.AlongAt(tick * (double)_step.Duration));
    }
}
