using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Rollback;

namespace Bw.Tests.Doubles
{
    public sealed class ServerStates<TState> : TickHistory<TState>, IReplicatedStates<TState> where TState : struct
    {
        public ServerStates(int capacity) : base(capacity)
        {
        }
    }
}
