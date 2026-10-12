using System;
using System.Collections.Generic;
using System.Text;
using Bw.Entities;
using Bw.Entities.Infrastructure;
using Bw.Entities.Network;
using Bw.Entities.Players;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Clients.Network
{
    public class NetworkClientCollection : IClientCollection //TODO: to project Context
    {
        private const int MaxNicknameBytes = NicknameRules.MaxLength * 4;

        public IViewableBiMap<ulong, IClient> ByIds { get; }

        private readonly Dictionary<ulong, string> _requestedNicknames = new();

        private NetworkClientCollection(Lifetime lifetime, NetworkManager manager, IRuntimeSettings runtimeSettings)
        {
            ByIds = new ViewableBiMap<ulong, IClient>(lifetime);
            if (runtimeSettings.CurrentPeerType != PeerType.Server)
            {
                return; // Only server handles spawning
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
            _requestedNicknames[request.ClientNetworkId] = RequestedNickname(request.Payload);
            response.Approved = true;
            response.CreatePlayerObject = false;
        }

        private void OnClientConnected(ulong id)
        {
            Debug.Log($"<color=cyan>Client {id} connected. Adding to client collection...</color>");
            var client = new Client(id, TakeRequestedNickname(id));
            ByIds.Add(id, client);
            client.ChangeState(ClientConnectionState.Connected);
        }

        private void OnClientDisconnected(ulong id)
        {
            Debug.Log($"<color=yellow>Client {id} disconnected</color>");
            _requestedNicknames.Remove(id);
            ByIds.Remove(id);
        }

        private string TakeRequestedNickname(ulong id)
        {
            if (!_requestedNicknames.Remove(id, out var nickname))
                throw new InvalidOperationException(
                    $"Client {id} connected without passing the approval: turn ConnectionApproval on in the NetworkManager config, the client's nickname comes with the connection request.");

            return nickname;
        }

        private static string RequestedNickname(byte[] payload) => //TODO: клиент пока ничего не кладёт в ConnectionData: поле имени стартового экрана не подключено, у всех NicknameRules.Fallback
            payload.Length is > 0 and <= MaxNicknameBytes
                ? NicknameRules.Accepted(Encoding.UTF8.GetString(payload))
                : NicknameRules.Fallback;
    }
}
