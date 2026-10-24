using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class ServerStateFollower<TState> where TState : struct
    {
        private readonly IReplicatedStates<TState> _server;
        private readonly IRestorable<TState> _state;

        private int _followedTick = int.MinValue;

        public ServerStateFollower(
            Lifetime lifetime,
            IPredictedBy predictedBy,
            IWorldStep worldStep,
            IReplicatedStates<TState> server,
            IRestorable<TState> state)
        {
            _server = server;
            _state = state;

            predictedBy.Me.WhenFalse(lifetime, followedLifetime =>
                worldStep.Stepped(TickPhase.Prepare).Advise(followedLifetime, _ => Follow()));
        }

        private void Follow()
        {
            var newest = _server.NewestTick;
            if (newest <= _followedTick || !_server.TryGetAt(newest, out var state))
                return;

            _followedTick = newest;
            _state.Apply(state);
        }
    }
}
