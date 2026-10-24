using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class PredictionStateBroadcaster<TState> where TState : struct
    {
        private readonly IReadonlySimulation<TState> _simulation;
        private readonly INetResultSender<TickedState<TState>> _result;

        private bool _changed;

        public PredictionStateBroadcaster(
            Lifetime lifetime,
            IWorldStep worldStep,
            IReadonlySimulation<TState> simulation,
            INetResultSender<TickedState<TState>> result)
        {
            _simulation = simulation;
            _result = result;

            simulation.Stepped.Advise(lifetime, _ => _changed = true);
            simulation.State.Advise(lifetime, _ => _changed = true);
            worldStep.Stepped(TickPhase.Publish).Advise(lifetime, Publish);
        }

        private void Publish(int tick)
        {
            if (!_changed)
                return;

            _changed = false;
            _result.Broadcast(new TickedState<TState>(tick, _simulation.State.Value));
        }
    }
}
