using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Replication
{
    public sealed class ReplicatedStates<TState> : IReplicatedStates<TState> where TState : struct
    {
        private const int MaxStates = 64;

        private readonly List<TickedState<TState>> _states = new();

        public ReplicatedStates(Lifetime lifetime, INetResultReceiver<TickedState<TState>> state)
        {
            state.Received.Advise(lifetime, Remember);
        }

        public bool TryGetAt(int tick, out TickedState<TState> state)
        {
            for (var index = _states.Count - 1; index >= 0; index--)
            {
                if (_states[index].Tick > tick)
                    continue;

                state = _states[index];
                return true;
            }

            state = default;
            return false;
        }

        private void Remember(TickedState<TState> state)
        {
            if (_states.Count > 0 && state.Tick <= _states[_states.Count - 1].Tick)
                return;

            _states.Add(state);
            if (_states.Count > MaxStates)
                _states.RemoveAt(0);
        }
    }
}
