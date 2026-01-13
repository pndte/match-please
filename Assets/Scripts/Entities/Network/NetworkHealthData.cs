using System;
using Unity.Netcode;
using UnityEngine;

namespace Entities.Network
{
    public class NetworkHealthData : NetworkLifetimedBehaviour
    {
        [NonSerialized] public NetworkVariable<float> Health;
        [SerializeField] private HealthConfig _healthConfig;

        protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
        {
            if (networkManager.IsServer)
                Health = new NetworkVariable<float>(_healthConfig.MaxHealth);
        }
    }
}