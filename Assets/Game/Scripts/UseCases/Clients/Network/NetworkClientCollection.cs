using System;
using Bw.Entities;
using Bw.Entities.Infrastructure;
using Bw.Entities.Network;
using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Clients.Network
{
    public class NetworkClientCollection : IClientCollection //TODO: to project Context
    {
        public IViewableBiMap<ulong, IClient> ByIds { get; }

        private NetworkClientCollection(Lifetime lifetime, NetworkManager manager, IRuntimeSettings runtimeSettings)
        {
            ByIds = new ViewableBiMap<ulong, IClient>(lifetime);
            if (runtimeSettings.CurrentPeerType != PeerType.Server)
            {
                return; // Only server handles spawning
            }
            
            manager.OnClientConnectedCallback += OnClientConnected;
            manager.OnClientDisconnectCallback += OnClientDisconnected;
            
            lifetime.OnTermination(() => manager.OnClientDisconnectCallback -= OnClientDisconnected);
            lifetime.OnTermination(() => manager.OnClientConnectedCallback -= OnClientConnected);
        }

        private void OnClientConnected(ulong id)
        {
            Debug.Log($"<color=cyan>Client {id} loading. Adding to client collection...</color>");
            var client = new Client(id);
            ByIds.Add(id, client);
            
            ChangeClientConnected(client).Forget();
        }

        private async UniTaskVoid ChangeClientConnected(IClient client)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(5));
            Debug.Log($"<color=cyan>Client {client.Id} connected. Adding to client collection...</color>");
            client.ChangeState(ClientConnectionState.Connected);
        }
        
        private void OnClientDisconnected(ulong id)
        {
            Debug.Log($"<color=yellow>Client {id} disconnected</color>");
            ByIds.Remove(id);
        }
    }
}