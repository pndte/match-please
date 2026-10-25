using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Rollback
{
    public sealed class WorldStep : IWorldStep
    {
        public IReadonlyProperty<bool> Replaying { get; } = new ViewableProperty<bool>(false);

        private readonly TickPhaseSignals _phases = new();

        public WorldStep(Lifetime lifetime, IWorldClock clock)
        {
            clock.Ticked.Advise(lifetime, Step);
        }

        public ISource<int> Stepped(TickPhase phase) =>
            _phases.Of(phase);

        private void Step(int tick)
        {
            _phases.Fire(TickPhase.Prepare, tick);
            _phases.Fire(TickPhase.Early, tick);
            _phases.Fire(TickPhase.Physics, tick);
            _phases.Fire(TickPhase.Default, tick);
            _phases.Fire(TickPhase.Late, tick);
            _phases.Fire(TickPhase.Publish, tick);
        }
    }
}
