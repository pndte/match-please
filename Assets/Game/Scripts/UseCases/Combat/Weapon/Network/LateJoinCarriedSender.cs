using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class LateJoinCarriedSender
    {
        private readonly INetworkLifetimedObject _networkObject;
        private readonly CarriedWeaponsReport _report;
        private readonly INetResultSender<CarriedWeapons> _carried;

        public LateJoinCarriedSender(
            Lifetime lifetime,
            INetworkLifetimedObject networkObject,
            IClientCollection clients,
            CarriedWeaponsReport report,
            INetResultSender<CarriedWeapons> carried)
        {
            _networkObject = networkObject;
            _report = report;
            _carried = carried;

            clients.ByIds.View(lifetime, (_, _, client) => SendCurrentTo(client));
        }

        private void SendCurrentTo(IClient client)
        {
            if (!_networkObject.SpawnedLifetime.Value.IsAlive)
                return;

            _carried.SendTo(client, _report.Current);
        }
    }
}
