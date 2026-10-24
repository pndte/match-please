using System;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Replication
{
    public sealed class StateChangeBroadcaster<TState> where TState : struct, IEquatable<TState>
    {
        private readonly IReadonlySimulation<TState> _simulation;
        private readonly INetResultSender<TickedState<TState>> _result;
        private readonly WorldRollbackConfig _config;

        private TState _sent;
        private int _sentTick;

        public StateChangeBroadcaster(
            Lifetime lifetime,
            INetworkTicks ticks,
            IWorldStep worldStep,
            IReadonlySimulation<TState> simulation,
            INetResultSender<TickedState<TState>> result,
            WorldRollbackConfig config)
        {
            _simulation = simulation;
            _result = result;
            _config = config;
            _sent = simulation.State.Value;
            _sentTick = ticks.Current;

            worldStep.Stepped(TickPhase.Publish).Advise(lifetime, Publish);
        }

        private void Publish(int tick)
        {
            var state = _simulation.State.Value;
            if (state.Equals(_sent))
            {
                if (tick - _sentTick >= _config.UnchangedStateTicks)
                    Confirm(tick, state);

                return;
            }

            if (_sentTick < tick - 1)
                _result.Broadcast(new TickedState<TState>(tick - 1, _simulation.Previous));

            Confirm(tick, state);
        }

        private void Confirm(int tick, TState state)
        {
            _result.Broadcast(new TickedState<TState>(tick, state));
            _sent = state;
            _sentTick = tick;
        }
    }
}
