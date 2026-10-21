using System;
using Bw.Entities.Players;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    internal interface INetReceiveGuard
    {
        bool TryAdmit(ulong senderClientId);
    }

    internal sealed class AllowedReceiveGuard : INetReceiveGuard
    {
        public bool TryAdmit(ulong senderClientId) => true;
    }

    internal sealed class OwnersOnlyReceiveGuard : INetReceiveGuard
    {
        private readonly IOwnerRegistry _ownerRegistry;
        private readonly StaleWriters _staleWriters;
        private readonly IClientCollection _clients;
        private readonly IReadonlyPlayerDrivers _drivers;
        private readonly NetworkObject _networkObject;

        public OwnersOnlyReceiveGuard(
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

        public bool TryAdmit(ulong senderClientId)
        {
            if (!_clients.ByIds.TryGetValue(senderClientId, out var client))
                throw NotOwnedBy(senderClientId);

            var player = _drivers.PlayerOf(client);
            if (IsOwner(player))
                return true;

            if (_staleWriters.Listed(player))
                return false;

            throw NotOwnedBy(senderClientId);
        }

        private bool IsOwner(IPlayer player)
        {
            var owners = _ownerRegistry.Owners;
            for (var index = 0; index < owners.Count; index++)
            {
                if (owners[index] == player)
                    return true;
            }

            return false;
        }

        private InvalidOperationException NotOwnedBy(ulong senderClientId) =>
            new($"Only an owner writes this entry, client {senderClientId.ToString()} doesn't own object {_networkObject.NetworkObjectId.ToString()}.");
    }

    internal sealed class ForbiddenReceiveGuard : INetReceiveGuard
    {
        private readonly string _reason;

        public ForbiddenReceiveGuard(string reason)
        {
            _reason = reason;
        }

        public bool TryAdmit(ulong senderClientId) =>
            throw new InvalidOperationException($"{_reason} Sender: client {senderClientId.ToString()}.");
    }
}
