using System;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    internal interface INetReceiveGuardFactory
    {
        INetReceiveGuard Create(NetworkPermissions permissions);
    }

    internal sealed class NetReceiveGuardFactoryClient : INetReceiveGuardFactory
    {
        public INetReceiveGuard Create(NetworkPermissions permissions) =>
            permissions switch
            {
                NetworkPermissions.Server => new AllowedReceiveGuard(),
                NetworkPermissions.Owner => new AllowedReceiveGuard(),
                NetworkPermissions.Client => new ForbiddenReceiveGuard("Entry with Client permissions is written only by its owning client, the server never sends it."),
                NetworkPermissions.Everyone => new AllowedReceiveGuard(),
                _ => throw new ArgumentOutOfRangeException(nameof(permissions), $"Unknown NetworkPermissions value {((byte)permissions).ToString()}.")
            };
    }

    internal sealed class NetReceiveGuardFactoryServer : INetReceiveGuardFactory
    {
        private readonly IOwnershipController _ownershipController;
        private readonly IClientCollection _clients;
        private readonly IClientPlayerCollection _clientPlayers;
        private readonly NetworkObject _networkObject;

        public NetReceiveGuardFactoryServer(
            IOwnershipController ownershipController,
            IClientCollection clients,
            IClientPlayerCollection clientPlayers,
            NetworkObject networkObject)
        {
            _ownershipController = ownershipController;
            _clients = clients;
            _clientPlayers = clientPlayers;
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
            new OwnersOnlyReceiveGuard(_ownershipController, _clients, _clientPlayers, _networkObject);
    }
}
