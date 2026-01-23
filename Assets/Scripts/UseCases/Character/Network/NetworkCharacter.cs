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
        public IViewableProperty<CharacterState> State { get; } = new ViewableProperty<CharacterState>(CharacterState.Alive);
        public IReadonlyProperty<Lifetime> Lifetime => SpawnedLifetime;
        IReadonlyProperty<CharacterState> IReadonlyCharacter.State => State; //TODO: sync by network
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
            State.Value = CharacterState.Dead;
            NetworkObject.Despawn(false); // TODO: refactor
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