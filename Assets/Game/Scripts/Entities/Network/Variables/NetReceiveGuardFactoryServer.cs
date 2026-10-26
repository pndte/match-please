using System;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    public sealed class NetReceiveGuardFactoryServer
    {
        private readonly IOwnerRegistry _ownerRegistry;
        private readonly StaleWriters _staleWriters;
        private readonly IClientCollection _clients;
        private readonly IReadonlyPlayerDrivers _drivers;
        private readonly NetworkObject _networkObject;

        public NetReceiveGuardFactoryServer(
            Lifetime lifetime,
            IOwnerRegistry ownerRegistry,
            IClientCollection clients,
            IReadonlyPlayerDrivers drivers,
            NetReceiveConfig receiveConfig,
            NetworkObject networkObject)
        {
            _ownerRegistry = ownerRegistry;
            _staleWriters = new StaleWriters(lifetime, ownerRegistry, receiveConfig.StaleWriterSeconds);
            _clients = clients;
            _drivers = drivers;
            _networkObject = networkObject;
        }

        internal INetReceiveGuard Create(NetworkPermissions permissions) =>
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
