using Bw.Entities;
using Bw.Entities.Infrastructure;
using JetBrains.Lifetimes;
using Setup;
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
            Debug.Log($"<color=cyan>Client {id} connected. Adding to client collection...</color>");
            ByIds.Add(id, new Client(id));
        }
        
        private void OnClientDisconnected(ulong id)
        {
            Debug.Log($"<color=yellow>Client {id} disconnected</color>");
            ByIds.Remove(id);
        }
    }
}