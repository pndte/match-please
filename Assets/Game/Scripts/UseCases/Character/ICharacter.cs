using Bw.Entities;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character
{
    public interface ICharacter : IReadonlyCharacter
    {
        public IHealth Health { get; }
        public IViewableProperty<CharacterState> State { get; }
        public void Die();
    }
    
    public interface IReadonlyCharacter
    {
        public IReadonlyHealth Health { get; }
        public IReadonlyProperty<CharacterState> State { get; }
        public IReadonlyProperty<Lifetime> Lifetime { get; }
    }
}