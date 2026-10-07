using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.UseCases.Character.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character.Network.Requests
{
    public sealed class LateJoinVitalsSender
    {
        private readonly INetworkLifetimedObject _networkObject;
        private readonly IReadonlyCharacter _character;
        private readonly IVitalsResult _vitals;

        public LateJoinVitalsSender(
            Lifetime lifetime,
            INetworkLifetimedObject networkObject,
            IClientCollection clients,
            IReadonlyCharacter character,
            IVitalsResult vitals)
        {
            _networkObject = networkObject;
            _character = character;
            _vitals = vitals;

            clients.ByIds.View(lifetime, (_, _, client) => SendCurrentTo(client));
        }

        private void SendCurrentTo(IClient client)
        {
            if (!_networkObject.SpawnedLifetime.Value.IsAlive)
                return;

            _vitals.Received.FireTo(client, _character.Vitals().WithoutChange());
        }
    }
}
