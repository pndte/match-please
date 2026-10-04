using System;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    internal interface INetSendGuardFactory
    {
        INetSendGuard Create(NetworkPermissions permissions);
    }

    internal sealed class NetSendGuardFactoryClient : INetSendGuardFactory
    {
        private readonly IOwnership _ownership;
        private readonly NetworkObject _networkObject;

        public NetSendGuardFactoryClient(IOwnership ownership, NetworkObject networkObject)
        {
            _ownership = ownership;
            _networkObject = networkObject;
        }

        public INetSendGuard Create(NetworkPermissions permissions) =>
            permissions switch
            {
                NetworkPermissions.Server => new ForbiddenSendGuard("Entry with Server permissions is sent only by the server, a client can't send it."),
                NetworkPermissions.Owner => WhileMine(),
                NetworkPermissions.Client => WhileMine(),
                NetworkPermissions.Everyone => new AllowedSendGuard(canTargetClients: false),
                _ => throw new ArgumentOutOfRangeException(nameof(permissions), permissions, "Unknown NetworkPermissions value.")
            };

        private INetSendGuard WhileMine() =>
            new WhileMineSendGuard(new AllowedSendGuard(canTargetClients: false), _ownership, _networkObject);
    }

    internal sealed class NetSendGuardFactoryServer : INetSendGuardFactory
    {
        private readonly IOwnership _ownership;
        private readonly NetworkObject _networkObject;

        public NetSendGuardFactoryServer(IOwnership ownership, NetworkObject networkObject)
        {
            _ownership = ownership;
            _networkObject = networkObject;
        }

        public INetSendGuard Create(NetworkPermissions permissions) =>
            permissions switch
            {
                NetworkPermissions.Server => new AllowedSendGuard(canTargetClients: true),
                NetworkPermissions.Owner => new WhileMineSendGuard(new AllowedSendGuard(canTargetClients: true), _ownership, _networkObject),
                NetworkPermissions.Client => new ForbiddenSendGuard("Entry with Client permissions is sent only by its owning client, the server can't send it."),
                NetworkPermissions.Everyone => new AllowedSendGuard(canTargetClients: true),
                _ => throw new ArgumentOutOfRangeException(nameof(permissions), permissions, "Unknown NetworkPermissions value.")
            };
    }
}
