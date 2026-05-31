using Bw.Entities;
using Bw.Entities.Network.Objects;
using JetBrains.Collections.Viewable;
using Unity.Netcode;

namespace Bw.UseCases.Character.Network
{
    public class ServerCharacter : ClientCharacter, ICharacter
    {
        private readonly NetworkObject _networkObject;
        public IHealth Health { get; }
        public IViewableProperty<CharacterState> State => base.State;
        
        public ServerCharacter(
            IHealth health, 
            INetworkLifetimedObject networkLifetimed,
            NetworkObject networkObject) : base(health, networkLifetimed)
        {
            _networkObject = networkObject;
            Health = health;
        }
        
        public void Die()
        {
            State.Value = CharacterState.Dead;
            _networkObject.Despawn(true); // TODO: убрать NetworkObject, сделать интерфейс
        }
    }
}