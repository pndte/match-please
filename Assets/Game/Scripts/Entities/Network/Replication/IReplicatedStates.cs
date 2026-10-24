using Bw.Entities.Network.Rollback;

namespace Bw.Entities.Network.Replication
{
    public interface IReplicatedStates<TState> : IReadonlyTickHistory<TState> where TState : struct
    {
    }
}
