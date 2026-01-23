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
        public IReadonlyProperty<Lifetime> Lifetime => _networkLifetimedBehaviour.SpawnedLifetime;
        IReadonlyHealth IReadonlyCharacter.Health => Health;

        private NetworkHealthData _networkHealthData;
        private NetworkLifetimedBehaviour _networkLifetimedBehaviour;

        [Inject]
        private void Construct(IHealth health, NetworkHealthData networkHealthData, NetworkLifetimedBehaviour networkLifetimedBehaviour)
        {
            Health = health;
            _networkHealthData = networkHealthData;
            _networkLifetimedBehaviour = networkLifetimedBehaviour;
        }
        
        public void Die()
        {
            _networkLifetimedBehaviour.NetworkObject.Despawn(true);
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