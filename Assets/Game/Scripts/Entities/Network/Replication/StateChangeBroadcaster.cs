using System;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Replication
{
    public sealed class StateChangeBroadcaster<TState> where TState : struct, IEquatable<TState>
    {
        private readonly INetworkTicks _ticks;
        private readonly IReadonlySimulation<TState> _simulation;
        private readonly INetResultSender<TickedState<TState>> _result;

        private TState _sent;
        private int _sentTick;

        public StateChangeBroadcaster(
            Lifetime lifetime,
            INetworkTicks ticks,
            IReadonlySimulation<TState> simulation,
            INetResultSender<TickedState<TState>> result)
        {
            _ticks = ticks;
            _simulation = simulation;
            _result = result;
            _sent = simulation.State.Value;
            _sentTick = ticks.Current;

            simulation.Stepped.Advise(lifetime, Broadcast);
        }

        private void Broadcast(TState state)
        {
            if (state.Equals(_sent))
                return;

            var tick = _ticks.Current;
            if (_sentTick < tick - 1)
                _result.Broadcast(new TickedState<TState>(tick - 1, _simulation.Previous));

            _result.Broadcast(new TickedState<TState>(tick, state));
            _sent = state;
            _sentTick = tick;
        }
    }
}
