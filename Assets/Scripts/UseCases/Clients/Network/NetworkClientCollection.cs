using System.Collections.Generic;
using Bw.Entities;
using JetBrains.Collections.Viewable;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Clients.Network
{
    public class NetworkClientCollection : NetworkBehaviour, IClientCollection
    {
        public IViewableMap<ulong, IClient> ByIds { get; } = new ViewableMap<ulong, IClient>();
        public ICollection<IClient> All => ByIds.Values;

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                return; // Only server handles spawning
            }
            
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
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