using Bw.Entities;
using Bw.Entities.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Character.Network
{
    public class NetworkCharacter : NetworkLifetimedBehaviour, ICharacter
    {
        public IHealth Health { get; private set; }
        public IReadonlyProperty<Lifetime> Lifetime => SpawnedLifetime;
        IReadonlyHealth IReadonlyCharacter.Health => Health;

        private NetworkHealthData _networkHealthData;

        [Inject]
        private void Construct(IHealth health, NetworkHealthData networkHealthData)
        {
            Health = health;
            _networkHealthData = networkHealthData;
        }
        
        public void Die()
        {
            NetworkObject.Despawn(true);
        }

        private void OnMouseDown()
        {
            if (!_networkHealthData.IsServer)
            {
                Debug.LogWarning($"[Client] No authority to modify health. IsServer: {_networkHealthData.IsServer}, IsOwner: {_networkHealthData.IsOwner}");
                return;
            }

            Debug.Log($"[{((_networkHealthData.IsServer) ? "Server" : "Client")}] Before Hit: {Health.Value}");
            Health.Value -= 10;
            Debug.Log($"[{((_networkHealthData.IsServer) ? "Server" : "Client")}] After Hit: {Health.Value}");
        }

    }
}