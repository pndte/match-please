using DefaultNamespace;
using Entities;

namespace UseCases.Network
{
    public interface ICharacter : IReadonlyCharacter
    {
        public IHealth Health { get; }
        public void Die();
    }
}