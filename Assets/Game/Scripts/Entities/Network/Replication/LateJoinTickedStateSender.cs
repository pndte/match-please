using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Replication
{
    public sealed class LateJoinTickedStateSender<TState> where TState : struct
    {
        private readonly INetworkLifetimedObject _networkObject;
        private readonly INetworkTicks _ticks;
        private readonly IReadonlySimulation<TState> _simulation;
        private readonly INetResultSender<TickedState<TState>> _result;

        public LateJoinTickedStateSender(
            Lifetime lifetime,
            INetworkLifetimedObject networkObject,
            IClientCollection clients,
            INetworkTicks ticks,
            IReadonlySimulation<TState> simulation,
            INetResultSender<TickedState<TState>> result)
        {
            _networkObject = networkObject;
            _ticks = ticks;
            _simulation = simulation;
            _result = result;

            clients.ByIds.View(lifetime, (_, _, client) => SendCurrentTo(client));
        }

        private void SendCurrentTo(IClient client)
        {
            if (!_networkObject.SpawnedLifetime.Value.IsAlive)
                return;

            _result.SendTo(client, new TickedState<TState>(_ticks.Current, _simulation.State.Value));
        }
    }
}
