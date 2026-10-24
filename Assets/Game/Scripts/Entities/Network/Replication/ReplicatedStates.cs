using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Replication
{
    public sealed class ReplicatedStates<TState> : TickHistory<TState>, IReplicatedStates<TState> where TState : struct
    {
        public ReplicatedStates(Lifetime lifetime, INetResultReceiver<TickedState<TState>> state, WorldRollbackConfig config)
            : base(config.HistoryTicks)
        {
            state.Received.Advise(lifetime, Remember);
        }

        private void Remember(TickedState<TState> state)
        {
            if (NewestTick == int.MinValue || state.Tick > NewestTick - Capacity)
                Record(state.Tick, state.State);
        }
    }
}
