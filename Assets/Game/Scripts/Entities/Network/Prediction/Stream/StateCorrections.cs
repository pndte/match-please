using System;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class StateCorrections<TState> : IStateCorrections<TState> where TState : struct, IEquatable<TState>
    {
        public ISource<StateCorrection<TState>> Corrected => _corrected;

        private readonly Signal<StateCorrection<TState>> _corrected = new();
        private readonly IReadonlySimulation<TState> _simulation;
        private readonly Action _compare;

        private TState _beforeReplay;

        public StateCorrections(Lifetime lifetime, IReadonlySimulation<TState> simulation, IWorldStep worldStep)
        {
            _simulation = simulation;
            _compare = Compare;

            worldStep.Replaying.WhenTrue(lifetime, Watch);
        }

        private void Watch(Lifetime replayLifetime)
        {
            _beforeReplay = _simulation.State.Value;
            replayLifetime.OnTermination(_compare);
        }

        private void Compare()
        {
            var after = _simulation.State.Value;
            if (!after.Equals(_beforeReplay))
                _corrected.Fire(new StateCorrection<TState>(_beforeReplay, after));
        }
    }
}
