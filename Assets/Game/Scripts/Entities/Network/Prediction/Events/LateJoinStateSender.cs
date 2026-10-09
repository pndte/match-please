using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class LateJoinStateSender<TState> where TState : struct
    {
        private readonly INetworkLifetimedObject _networkObject;
        private readonly IReadonlyAppliedState<TState> _state;
        private readonly INetResultSender<CausedState<TState>> _result;

        public LateJoinStateSender(
            Lifetime lifetime,
            INetworkLifetimedObject networkObject,
            IClientCollection clients,
            IReadonlyAppliedState<TState> state,
            INetResultSender<CausedState<TState>> result)
        {
            _networkObject = networkObject;
            _state = state;
            _result = result;

            clients.ByIds.View(lifetime, (_, _, client) => SendCurrentTo(client));
        }

        private void SendCurrentTo(IClient client)
        {
            if (!_networkObject.SpawnedLifetime.Value.IsAlive)
                return;

            _result.SendTo(client, new CausedState<TState>(_state.Current, StateCause.None));
        }
    }
}
