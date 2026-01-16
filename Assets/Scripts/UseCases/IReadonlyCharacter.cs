using Entities;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DefaultNamespace
{
    public interface IReadonlyCharacter
    {
        public IReadonlyHealth Health { get; }
        public IReadonlyProperty<Lifetime> Lifetime { get; }
    }
}