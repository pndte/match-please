using Bw.Entities;
using Bw.Entities.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character.Network
{
    public class ClientCharacter : IReadonlyCharacter
    {
        public IReadonlyHealth Health { get; }
        public IReadonlyProperty<Lifetime> Lifetime => _networkLifetimed.SpawnedLifetime;
        IReadonlyProperty<CharacterState> IReadonlyCharacter.State => State;
        
        protected ViewableProperty<CharacterState> State { get; } = new(CharacterState.Alive);
        
        private readonly INetworkLifetimed _networkLifetimed;

        public ClientCharacter(
            IReadonlyHealth health,
            INetworkLifetimed networkLifetimed)
        {
            Health = health;
            _networkLifetimed = networkLifetimed;
        }
    }
}