using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network
{
    public interface ILocalPlayer
    {
        public IReadonlyProperty<LocalPlayerId> Id { get; }
        public void Set(Lifetime lifetime, int id);
    }
}
