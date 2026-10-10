using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class LateJoinWeaponDrawSender
    {
        private readonly INetworkLifetimedObject _networkObject;
        private readonly IReadonlyWeaponDraw _draw;
        private readonly INetResultSender<bool> _drawn;

        public LateJoinWeaponDrawSender(
            Lifetime lifetime,
            INetworkLifetimedObject networkObject,
            IClientCollection clients,
            IReadonlyWeaponDraw draw,
            INetResultSender<bool> drawn)
        {
            _networkObject = networkObject;
            _draw = draw;
            _drawn = drawn;

            clients.ByIds.View(lifetime, (_, _, client) => SendCurrentTo(client));
        }

        private void SendCurrentTo(IClient client)
        {
            if (!_networkObject.SpawnedLifetime.Value.IsAlive || !_draw.DrawnLifetime.Value.IsAlive)
                return;

            _drawn.SendTo(client, true);
        }
    }
}
