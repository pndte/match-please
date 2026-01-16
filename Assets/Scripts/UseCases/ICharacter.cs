using Entities;

namespace DefaultNamespace
{
    public interface ICharacter : IReadonlyCharacter
    {
        public IHealth Health { get; }
    }

    public interface IReadonlyCharacter
    {
        public IReadonlyHealth Health { get; }
    }
}