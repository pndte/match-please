using System;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    internal interface INetReceiveGuard
    {
        void Check(ulong senderClientId);
    }

    internal sealed class AllowedReceiveGuard : INetReceiveGuard
    {
        public void Check(ulong senderClientId)
        {
        }
    }

    internal sealed class OwnersOnlyReceiveGuard : INetReceiveGuard
    {
        private readonly IOwnershipController _ownershipController;
        private readonly IClientCollection _clients;
        private readonly IClientPlayerCollection _clientPlayers;
        private readonly NetworkObject _networkObject;

        public OwnersOnlyReceiveGuard(
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

        public void Check(ulong senderClientId)
        {
            if (!SenderOwns(senderClientId)) //TODO: честная гонка — ввод, отправленный до снятия владения (выброс оружия, смерть), ещё в пути: ввод идёт каждый тик, и сервер бросает на каждый такой пакет — отличать гонку от нарушения
                throw new InvalidOperationException(
                    $"Only an owner writes this entry, client {senderClientId.ToString()} doesn't own object {_networkObject.NetworkObjectId.ToString()}.");
        }

        private bool SenderOwns(ulong senderClientId)
        {
            if (!_clients.ByIds.TryGetValue(senderClientId, out var client)
                || !_clientPlayers.ByClient.TryGetValue(client, out var player))
                return false;

            var owners = _ownershipController.Owners;
            for (var index = 0; index < owners.Count; index++)
            {
                if (owners[index] == player)
                    return true;
            }

            return false;
        }
    }

    internal sealed class ForbiddenReceiveGuard : INetReceiveGuard
    {
        private readonly string _reason;

        public ForbiddenReceiveGuard(string reason)
        {
            _reason = reason;
        }

        public void Check(ulong senderClientId) =>
            throw new InvalidOperationException($"{_reason} Sender: client {senderClientId.ToString()}.");
    }
}
