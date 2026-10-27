using System;
using System.Collections.Generic;
using Bw.CoreAdapters.Characters;
using Bw.Entities;
using Bw.Entities.Infrastructure;
using Bw.Entities.Network;
using Bw.Entities.Players;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Clients.Network
{
    public sealed class NetworkClientCollection : IClientCollection
    {
        public IViewableBiMap<ulong, IClient> ByIds { get; }

        private readonly CharacterCatalog _characters;
        private readonly string _level;
        private readonly Dictionary<ulong, ConnectionRequest> _requests = new();

        private NetworkClientCollection(
            Lifetime lifetime,
            NetworkManager manager,
            IRuntimeSettings runtimeSettings,
            CharacterCatalog characters)
        {
            _characters = characters;
            _level = runtimeSettings.Level;
            ByIds = new ViewableBiMap<ulong, IClient>(lifetime);
            if (runtimeSettings.CurrentPeerType != PeerType.Server)
            {
                return;
            }

            manager.ConnectionApprovalCallback = Approve;
            manager.OnClientConnectedCallback += OnClientConnected;
            manager.OnClientDisconnectCallback += OnClientDisconnected;

            lifetime.OnTermination(() => manager.OnClientDisconnectCallback -= OnClientDisconnected);
            lifetime.OnTermination(() => manager.OnClientConnectedCallback -= OnClientConnected);
            lifetime.OnTermination(() => manager.ConnectionApprovalCallback = null);
        }

        private void Approve(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (LevelQuery.IsAsked(request.Payload))
            {
                response.Approved = false;
                response.Reason = LevelQuery.Answer(_level);
                return;
            }

            _requests[request.ClientNetworkId] = Accepted(request.Payload);
            response.Approved = true;
            response.CreatePlayerObject = false;
        }

        private void OnClientConnected(ulong id)
        {
            Debug.Log($"<color=cyan>Client {id} connected. Adding to client collection...</color>");
            ByIds.Add(id, new Client(TakeRequest(id)));
        }

        private void OnClientDisconnected(ulong id)
        {
            Debug.Log($"<color=yellow>Client {id} disconnected</color>");
            _requests.Remove(id);
            ByIds.Remove(id);
        }

        private ConnectionRequest TakeRequest(ulong id)
        {
            if (!_requests.Remove(id, out var request))
                throw new InvalidOperationException(
                    $"Client {id} connected without passing the approval: turn ConnectionApproval on in the NetworkManager config, the client's nickname and character come with the connection request.");

            return request;
        }

        private ConnectionRequest Accepted(byte[] payload) =>
            ConnectionRequestPayload.TryRead(payload, out var requested)
                ? new ConnectionRequest(NicknameRules.Accepted(requested.Nickname), _characters.Accepted(requested.CharacterKey).Key, requested.Token, requested.HostKey)
                : new ConnectionRequest(NicknameRules.Fallback, _characters.DefaultCharacter().Key, Guid.NewGuid(), Guid.Empty);
    }
}
