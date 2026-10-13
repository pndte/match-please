using Bw.Entities.Network.Prediction.Stream.Requests;

namespace Bw.Entities.Network.Replication
{
    public interface IReplicatedStates<TState> where TState : struct
    {
        public bool TryGetAt(int tick, out TickedState<TState> state);
    }
}
