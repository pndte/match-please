using System;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    internal sealed class NetReceiveGuardFactoryServer
    {
        private readonly IOwnerRegistry _ownerRegistry;
        private readonly StaleWriters _staleWriters;
        private readonly IClientCollection _clients;
        private readonly IReadonlyPlayerDrivers _drivers;
        private readonly NetworkObject _networkObject;

        public NetReceiveGuardFactoryServer(
            IOwnerRegistry ownerRegistry,
            StaleWriters staleWriters,
            IClientCollection clients,
            IReadonlyPlayerDrivers drivers,
            NetworkObject networkObject)
        {
            _ownerRegistry = ownerRegistry;
            _staleWriters = staleWriters;
            _clients = clients;
            _drivers = drivers;
            _networkObject = networkObject;
        }

        public INetReceiveGuard Create(NetworkPermissions permissions) =>
            permissions switch
            {
                NetworkPermissions.Server => new ForbiddenReceiveGuard("Entry with Server permissions is written only by the server, a client can't send it."),
                NetworkPermissions.Owner => OwnersOnly(),
                NetworkPermissions.Client => OwnersOnly(),
                NetworkPermissions.Everyone => new AllowedReceiveGuard(),
                _ => throw new ArgumentOutOfRangeException(nameof(permissions), $"Unknown NetworkPermissions value {((byte)permissions).ToString()}.")
            };

        private INetReceiveGuard OwnersOnly() =>
            new OwnersOnlyReceiveGuard(_ownerRegistry, _staleWriters, _clients, _drivers, _networkObject);
    }
}
