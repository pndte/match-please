using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Rollback
{
    public sealed class WorldStep : IWorldStep
    {
        public IReadonlyProperty<bool> Replaying { get; } = new ViewableProperty<bool>(false);

        private readonly TickPhaseSignals _phases = new();
        private readonly ISteppable _physics;

        public WorldStep(Lifetime lifetime, IWorldClock clock, ISteppable physics)
        {
            _physics = physics;

            clock.Ticked.Advise(lifetime, Step);
        }

        public ISource<int> Stepped(TickPhase phase) =>
            _phases.Of(phase);

        private void Step(int tick)
        {
            _phases.Fire(TickPhase.Prepare, tick);
            _phases.Fire(TickPhase.Early, tick);
            _physics.Step();
            _phases.Fire(TickPhase.Default, tick);
            _phases.Fire(TickPhase.Late, tick);
            _phases.Fire(TickPhase.Publish, tick);
        }
    }
}
